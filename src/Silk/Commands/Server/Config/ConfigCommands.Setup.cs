using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Localisation;
using Mediator;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Extensions.Remora;
using Silk.Shared.Constants;
using Silk.Utilities;

namespace Silk.Commands.Server;

public partial class ConfigCommands
{
    public class Setup : CommandGroup
    {
        private const string SetupDescription =
            """
            Confused about what to configure?
            This command will automatically configure that're suitable for *most* servers.
            The following settings will be configured:
                ➜ Log member joins
                ➜ Log message deletes
                ➜ Enable invite whitelist
                ➜ Enable infractioin logging
                ➜ Detect, delete, and ban phishing messages
            """;

       
        private const string ModLogChannelTopic = 
            """
            Moderation logs for Silk!
            All actions taken by moderators will be logged here
            This channel is automatically created when the bot is configured.
            You can delete it if you wish.
            """;
        
        private const string ModLogChannelName = "mod-log";

        private const string ModLogChannelReason = "Automatically created via config setup command.";
        private const string ModLogPermissionReason = "Adjusting permissions for mod-log channel.";
        
        private const string InProgressTitle = "**Configuration in progress!**\n\n";
        private const string ProgressFailedTitle = "**Configuration failed!**\n\n";

        private static readonly DiscordPermissionSet _modlogPermissions = new(DiscordPermission.ViewChannel, DiscordPermission.ReadMessageHistory);

        private readonly IMediator              _mediator;
        private readonly ITextCommandContext         _context;
        private readonly IDiscordRestUserAPI    _users;
        private readonly IDiscordRestGuildAPI   _guilds;
        private readonly IDiscordRestChannelAPI _channels;
        
        public Setup(IMediator mediator, ITextCommandContext context, IDiscordRestUserAPI users, IDiscordRestGuildAPI guilds, IDiscordRestChannelAPI channels)
        {
            _mediator = mediator;
            _context  = context;
            _users    = users;
            _guilds   = guilds;
            _channels = channels;
        }


        [Command("setup")]
        [Description(SetupDescription)]
        [RequireDiscordPermission(DiscordPermission.ManageChannels)]
        [RequireBotDiscordPermissions(DiscordPermission.ManageChannels)]
        public async Task<Result> SetupAsync
        (
            [Description("The (optional) channel to send logs to! One will be created automatically otherwise.")]
            //[RequireBotDiscordPermissions(DiscordPermission.SendMessages, DiscordPermission.EmbedLinks)] 
            IChannel? channel = null
        )
        {
            var config = await _mediator.Send(new GetGuildConfig.Request(_context.GuildID.Value, true));
            
            if (config is null)
                return Result.FromSuccess();
            
            var now = DateTimeOffset.UtcNow;
            
            var sb = new LineBuilder();
            var messageRes = await _channels.CreateMessageAsync(_context.GetChannelID(), InProgressTitle);

            if (!messageRes.IsDefined(out var message))
                return (Result) messageRes;
            
            sb.AppendLine("\t ➜ Setting up mod-log channel....");

            await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, InProgressTitle + sb);

            var currentStep = await CreateModLogChannelAsync();
            
            await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, InProgressTitle + sb);

            if (!currentStep.IsSuccess)
            {
                return (Result)await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, ProgressFailedTitle + sb);
            }

            sb.Commit();
            sb.AppendLine();
            sb.AppendLine("\t ➜ Setting up mod-log channel permissions....");
            
            await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, InProgressTitle + sb);
            
            currentStep = await SetModLogChannelPermissionsAsync();
            
            await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, InProgressTitle + sb);
            
            if (!currentStep.IsSuccess)
            {
                return (Result)await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, ProgressFailedTitle + sb);
            }
            
            sb.Commit();
            sb.AppendLine();
            sb.AppendLine("\t ➜ Setting logging channel to mod-log channel...");
            await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, InProgressTitle + sb);
            
            currentStep = await SetLoggingChannelAsync();
            
            await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, InProgressTitle + sb);
            
            if (!currentStep.IsSuccess)
            {
                return (Result)await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, ProgressFailedTitle + sb);
            }
            
            sb.Commit();
            sb.AppendLine();
            sb.AppendLine("\t ➜ Setting up invite whitelist...");
            
            await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, InProgressTitle + sb);
            
            currentStep = await SetInviteWhitelistAsync();
            
            await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, InProgressTitle + sb);
            
            if (!currentStep.IsSuccess)
            {
                return (Result)await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, ProgressFailedTitle + sb);
            }
            
            sb.Commit();
            sb.AppendLine();
            sb.AppendLine("\t ➜ Setting up infraction logging...");
            
            await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, InProgressTitle + sb);
            
            currentStep = await SetInfractionLoggingAsync();
            
            await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, InProgressTitle + sb);
            
            if (!currentStep.IsSuccess)
            {
                return (Result)await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, ProgressFailedTitle + sb);
            }
            
            sb.Commit();
            sb.AppendLine();
            sb.AppendLine("\t ➜ Setting up phishing detection...");
            
            await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, InProgressTitle + sb);
            
            currentStep = await SetPhishingDetectionAsync();
            
            await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, InProgressTitle + sb);

            if (!currentStep.IsSuccess)
            {
                return (Result)await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, ProgressFailedTitle + sb);
            }

            var finish = DateTimeOffset.UtcNow;

            sb.AppendLine();
            sb.AppendLine("\t ➜ Configuration complete!");
            sb.AppendLine($"\t ➜ Took {(finish - now).Humanize(2, minUnit: TimeUnit.Second)} to complete.");

            sb.AppendLine();
            sb.AppendLine("Use `config view` to view the current configuration.");
            sb.AppendLine("For more information, see the complete setup guide: <https://blog.velvetthepanda.dev/newcomers-of-silk>");
            
            await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, InProgressTitle + sb);

            async Task<Result> CreateModLogChannelAsync()
            {
                if (channel is not null)
                {
                    sb.AppendLine("\t\t ➜ Channel specified, skipping initialization...");
                    await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, InProgressTitle + sb);
                }
                else
                {
                    sb.AppendLine("\t\t ➜ No channel specified, creating one...");
                    await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, InProgressTitle + sb);
                    
                    var currentChannel = await _channels.GetChannelAsync(_context.GetChannelID());

                    var channelResult = await _guilds.CreateGuildChannelAsync
                    (
                     _context.GuildID.Value,
                     name: ModLogChannelName,
                     topic: ModLogChannelTopic,
                     reason: ModLogChannelReason,
                     parentID: currentChannel.Entity.ParentID,
                     ct: CancellationToken
                    );

                    if (channelResult.IsDefined(out channel))
                    {
                        sb.RemoveLine();
                        sb.AppendLine($"\t\t ➜ Channel created! (<#{channel.ID}>)");
                    }
                    else
                    {
                        sb.AppendLine("\t\t ➜ Failed to create channel!");
                        return (Result)channelResult;
                    }
                    
                    await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, InProgressTitle + sb);
                }

                return Result.FromSuccess();
            }

            async Task<Result> SetModLogChannelPermissionsAsync()
            {
                var roles = await _guilds.GetGuildRolesAsync(_context.GuildID.Value, ct: CancellationToken);

                if (!roles.IsSuccess)
                    return (Result)roles;
            
                var member = await _guilds.GetGuildMemberAsync(_context.GuildID.Value, _context.GetUserID(), ct: CancellationToken);
                
                if (!member.IsSuccess)
                    return (Result)member;

                var self = await _guilds.GetCurrentGuildMemberAsync(_users, _context.GuildID.Value);
                
                if (!self.IsSuccess)
                    return (Result)self;
                
                var rolesDictionary = roles.Entity.ToDictionary(x => x.ID, x => x);

                var applicableRole = roles.Entity
                                           .Where(r => self.Entity.Roles.Any() && rolesDictionary[self.Entity.Roles.Last()].Position < r.Position)
                                           .LastOrDefault(
                                                          r =>
                                                          {
                                                              var perms = DiscordPermissionSet.ComputePermissions(r.ID, rolesDictionary[_context.GuildID.Value]);
                                                              
                                                              // Admin get access to the channel anyway, so don't add them.
                                                              return perms.HasPermission(DiscordPermission.KickMembers) && !perms.HasPermission(DiscordPermission.Administrator);
                                                          }
                                                         );

                sb.AppendLine("\t\t ➜ Locking channel for everyone...");

                var editResult = await _channels.EditChannelPermissionsAsync
                (
                 channel!.ID,
                 _context.GuildID.Value,
                 default,
                 _modlogPermissions,
                 reason: ModLogPermissionReason,
                 ct: CancellationToken
                );

                if (editResult.IsSuccess)
                {
                    sb.RemoveLine();
                    sb.AppendLine("\t\t ➜ Locked channel for everyone!");
                }
                else
                {
                    sb.AppendLine("\t\t ➜ Failed to lock channel for everyone!");
                    return editResult;
                }

                sb.AppendLine("\t\t ➜ Setting overrides for moderators...");


                if (applicableRole is not null)
                {
                    // We can't add the invoker to the channel; they're usually above us.
                    editResult = await _channels.EditChannelPermissionsAsync
                    (
                     channel.ID,
                     applicableRole!.ID,
                     _modlogPermissions,
                     reason: ModLogPermissionReason,
                     ct: CancellationToken
                    );
                }

                if (editResult.IsSuccess)
                {
                    sb.RemoveLine();
                    sb.AppendLine("\t\t ➜ Overrides set for moderators!");
                }
                else
                {
                    sb.AppendLine("\t\t ➜ Failed to set overrides for moderators!");
                    return editResult;
                }

                return Result.FromSuccess();
            }

            async Task<Result> SetLoggingChannelAsync()
            {
                sb.AppendLine("\t\t ➜ Setting join and deletes channel to mod-log channel...");

                config.Logging.LogMemberJoins = true;
                config.Logging.LogMessageDeletes = true;

                config.Logging.MemberJoins    = new() { ChannelID = channel!.ID, GuildID = _context.GuildID.Value };
                config.Logging.MessageDeletes = new() { ChannelID = channel!.ID, GuildID = _context.GuildID.Value };

                try
                {
                    config = await _mediator.Send(new UpdateGuildConfig.Request(_context.GuildID.Value) 
                    {
                        LoggingConfig = config.Logging,
                        ShouldCommit  = false
                    });

                    sb.AppendLine("\t\t ➜ Join and delete logging channels set!");
                    return Result.FromSuccess();
                }
                catch (Exception e)
                {
                    sb.AppendLine("\t\t ➜ Failed to set join and delete logging channels!");
                    return e;
                }
            }

            async Task<Result> SetInviteWhitelistAsync()
            {
                sb.AppendLine("\t\t ➜ Enabling invite whitelist...");
                await _channels.EditMessageAsync(_context.GetChannelID(), message.ID, InProgressTitle + sb);

                config = await _mediator.Send(new UpdateGuildConfig.Request(_context.GuildID.Value)
                {
                    ScanInvites           = true,
                    BlacklistInvites      = true,
                    WarnOnMatchedInvite   = true,
                    DeleteOnMatchedInvite = true,
                    ShouldCommit          = false
                });

                sb.RemoveLine();
                sb.AppendLine("\t\t ➜ Invite whitelist enabled!");

                return Result.FromSuccess();
            }

            async Task<Result> SetInfractionLoggingAsync()
            {
                sb.AppendLine("\t\t ➜ Enabling infraction logging...");

                config.Logging.LogInfractions = true;
                config.Logging.Infractions    = new() { ChannelID = channel!.ID, GuildID = _context.GuildID.Value };

                try
                {
                    config = await _mediator.Send(new UpdateGuildConfig.Request(_context.GuildID.Value)
                    {
                        LoggingConfig = config.Logging,
                        ShouldCommit  = false
                    });

                    sb.RemoveLine();
                    sb.AppendLine("\t\t ➜ Infraction logging enabled!");
                    return Result.FromSuccess();
                }
                catch (Exception e)
                {
                    sb.AppendLine("\t\t ➜ Failed to enable infraction logging!");
                    return e;
                }
            }

            async Task<Result> SetPhishingDetectionAsync()
            {
                sb.AppendLine("\t\t ➜ Enabling phishing detection...");

                config.NamedInfractionSteps.Add(AutoModConstants.PhishingLinkDetected, new() { Type = InfractionType.Ban });

                config = await _mediator.Send(new UpdateGuildConfig.Request(_context.GuildID.Value)
                {
                    DeletePhishingLinks    = true,
                    DetectPhishingLinks    = true,
                    BanSuspiciousUsernames = true,
                    InfractionSteps        = config.NamedInfractionSteps.Values.ToList()
                });

                sb.RemoveLine();
                sb.AppendLine("\t\t ➜ Phishing detection enabled!");

                return Result.FromSuccess();
            }

            return Result.FromSuccess();
        }
    }
}
