//TODO: This

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Humanizer;
using MediatR;
using OneOf;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Data.MediatR.Guilds.Config;
using Silk.Extensions;
using Silk.Extensions.Remora;
using Silk.Services.Data;
using Silk.Shared.Constants;
using Silk.Utilities.HelpFormatter;

namespace Silk.Commands.Server;

[Group("config", "cfg")]
[HelpCategory(Categories.Server)]
[RequireContext(ChannelContext.Guild)]
[Description("Configure various settings for your server!")]
[RequireDiscordPermission(DiscordPermission.ManageMessages, DiscordPermission.KickMembers, Operator = LogicalOperator.And)]
public class ConfigCommands : CommandGroup
{
    private readonly ICommandContext         _context;
    private readonly GuildConfigCacheService _configCache;
    private readonly IDiscordRestChannelAPI  _channels;

    public ConfigCommands(ICommandContext context, GuildConfigCacheService configCache, IDiscordRestChannelAPI channels)
    {
        _context     = context;
        _configCache = configCache;
        _channels    = channels;
    }

    [Command("reload")]
    [Description("Reloads the configuration for your server.")]
    public async Task<Result<IMessage>> ReloadConfigAsync()
    {
        _configCache.PurgeCache(_context.GuildID.Value);
        
        // If this fails it doesn't matter. Don't even await it.
        _ = _channels.CreateReactionAsync(_context.ChannelID, (_context as MessageContext)!.MessageID, $"_:{Emojis.ConfirmId}");
        
        return await _channels.CreateMessageAsync(_context.ChannelID, $"{Emojis.ReloadEmoji} Config reloaded! Changes should take effect immediately.");
    }

    [Group("view", "v")]
    [Description("View the settings for your server.")]
    public class ViewConfigCommands : CommandGroup
    {
        private readonly IMediator              _mediator;
        private readonly MessageContext         _context;
        private readonly IDiscordRestGuildAPI   _guilds;
        private readonly IDiscordRestChannelAPI _channels;

        public ViewConfigCommands
        (
            IMediator              mediator,
            MessageContext         context,
            IDiscordRestGuildAPI   guilds,
            IDiscordRestChannelAPI channels
        )
        {
            _mediator = mediator;
            _context  = context;
            _guilds   = guilds;
            _channels = channels;
        }
        
        //TODO: Add exmemptions
        
        [Command("all", "a")]
        [Description("View all settings for your server.")]
        public async Task<IResult> ViewAllAsync()
        {
            var config    = await _mediator.Send(new GetGuildConfig.Request(_context.GuildID.Value));
            var modConfig = await _mediator.Send(new GetGuildModConfig.Request(_context.GuildID.Value));

            var guildResult = await _guilds.GetGuildAsync(_context.GuildID.Value);

            if (!guildResult.IsDefined(out var guild))
                return guildResult;

            var phishingAction = modConfig.NamedInfractionSteps!.TryGetValue(AutoModConstants.PhishingLinkDetected, out var action) ? action.Type.ToString() : "Not configured";
            
            var contentBuilder = new StringBuilder();

            contentBuilder
               .Clear()
               .AppendLine($"{Emojis.SettingsEmoji} **General Config:**")
               .AppendLine("__Greetings__ | `config edit greetings`")
               .AppendLine($"> Configured Greetings: {config.Greetings.Count}")
               .AppendLine()
               .AppendLine($"{Emojis.WrenchEmoji} **Moderation Config:**")
               .AppendLine()
               .AppendLine("__Logging__ | Soon:tm:")
               .AppendLine($"> {(modConfig.Logging.LogMemberJoins ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.JoinEmoji} Log members joining")
               .AppendLine($"> {(modConfig.Logging.LogMemberLeaves ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.LeaveEmoji} Log members leaving")
               .AppendLine($"> {(modConfig.Logging.LogMessageEdits ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.EditEmoji} Log message edits")
               .AppendLine($"> {(modConfig.Logging.LogMessageDeletes ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.DeleteEmoji} Log message deletes")
               .AppendLine()
               .AppendLine("__Invites__ | `config edit invites`, `config edit invite-whitelist`")
               .AppendLine($"> {(modConfig.Invites.ScanOrigin ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.ScanEmoji} Scan invite origin")
               .AppendLine($"> {(modConfig.Invites.WarnOnMatch ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.WarningEmoji} Warn on invite")
               .AppendLine($"> {(modConfig.Invites.DeleteOnMatch ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.DeleteEmoji} Delete matched invite")
               .AppendLine($"> {(modConfig.UseAggressiveRegex ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.NoteEmoji} Use aggressive invite matching")
               .AppendLine($"> Allowed invites: {(modConfig.Invites.Whitelist.Count is 0 ? "None" : $"{modConfig.Invites.Whitelist.Count} allowed invites [See `config view invites`]")}")
               .AppendLine()
               .AppendLine("__Infractions__ | `config edit infractions`")
               .AppendLine($"> Mute role: {(modConfig.MuteRoleID.Value is 0 ? "Not set" : $"<@&{modConfig.MuteRoleID}>")}")
               .AppendLine($"> {(modConfig.UseNativeMute ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} Use native mutes (Requires Timeout Members permission)")
               .AppendLine($"> {(modConfig.ProgressiveStriking ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} Escalate infractions")
               .AppendLine($"> Infraction steps: {(modConfig.InfractionSteps.Count is var dictCount and not 0 ? $"{dictCount} steps [See `config view infractions`]" : "Not configured")}")
               .AppendLine($"> Infraction steps (named): {((modConfig.NamedInfractionSteps?.Count ?? 0) is var infNameCount and not 0 ? $"{infNameCount} steps [See `config view infractions`]" : "Not configured")}")
               .AppendLine()
               .AppendLine($"__Anti-Phishing__ {Emojis.NewEmoji} | `config edit phishing`")
               .AppendLine($"> {(modConfig.DetectPhishingLinks ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.WarningEmoji} Detect Phishing Links")
               .AppendLine($"> {(modConfig.DeletePhishingLinks ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.DeleteEmoji} Delete Phishing Links")
               .AppendLine($"> {(action is not null ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.WrenchEmoji} Post-detection action: {phishingAction}");

            var embed = new Embed
            {
                Title       = $"Config for {guild.Name}!",
                Colour      = Color.Goldenrod,
                Description = contentBuilder.ToString()
            };

            return await _channels.CreateMessageAsync(_context.ChannelID, embeds: new[] { embed });
        }

        [Command("phishing", "p")]
        [Description("View Anti-Phishing settings for your server.")]
        public async Task<IResult> ViewPhishingAsync()
        {
            var config = await _mediator.Send(new GetGuildModConfig.Request(_context.GuildID.Value));

            var guildResult = await _guilds.GetGuildAsync(_context.GuildID.Value);

            if (!guildResult.IsDefined(out var guild))
                return guildResult;

            //I don't like how long this line is
            var actionType = config.NamedInfractionSteps.TryGetValue(AutoModConstants.PhishingLinkDetected, out var phishingAction) ? phishingAction.Type.Humanize() : "Not configured";
            
            var enabled    = config.DeletePhishingLinks ? Emojis.EnabledEmoji : Emojis.DisabledEmoji;
            var delete     = config.DeletePhishingLinks ? Emojis.EnabledEmoji : Emojis.DisabledEmoji;
            
            var action = phishingAction is not null ? Emojis.EnabledEmoji : Emojis.DisabledEmoji;
            
            var embed = new Embed
            {
                Colour = Color.MidnightBlue,
                Title  = $"Phishing detection for {guild.Name}",
                Description = $"{enabled} {Emojis.WarningEmoji} **Detect Phishing Links** \n"  +
                              $"{delete} {Emojis.DeleteEmoji} **Delete Phishing Links**  \n" +
                              $"{action} {Emojis.WrenchEmoji} **After-detection action :** {actionType}"
            };

            return await _channels.CreateMessageAsync(_context.ChannelID, embeds: new[] { embed });
        }

        [Command("infractions", "i")]
        [Description("View Infraction settings for your server.")]
        public async Task<IResult> ViewInfractionsAsync()
        {
            var config = await _mediator.Send(new GetGuildModConfig.Request(_context.GuildID.Value));

            var guildResult = await _guilds.GetGuildAsync(_context.GuildID.Value);

            if (!guildResult.IsDefined(out var guild))
                return guildResult;

            var muteRole     = config.MuteRoleID.Value is 0 ? "Not configured." : $"<@&{config.MuteRoleID}>";
            var autoEscalate = config.ProgressiveStriking ? Emojis.EnabledEmoji : Emojis.DisabledEmoji;
            var infractionSteps = !config.InfractionSteps.Any()
                ? "Not configured."
                : config
                 .InfractionSteps
                 .Select((inf, ind) => $"{ind + 1} ➜ {inf.Type.Humanize()}")
                 .Join("\n");

            var infractionStepsNamed = !config.NamedInfractionSteps.Any()
                ? "Not configured."
                : config
                 .NamedInfractionSteps
                 .Select(inf => $"{AutoModConstants.ActionStrings[inf.Key]} ({inf.Key}) ➜ {inf.Value.Type.Humanize()}")
                 .Join("\n");

            var embed = new Embed
            {
                Colour = Color.MidnightBlue,
                Title  = $"Infraction settings for {guild.Name}",
                Description = $"**Mute Role:** {muteRole}\n"                +
                              $"{autoEscalate} **Automatically Escalate**\n" +
                              $"**Infraction steps:** {infractionSteps}\n"  +
                              $"**Infraction steps (named):** {infractionStepsNamed}"
            };

            return await _channels.CreateMessageAsync(_context.ChannelID, embeds: new[] { embed });
        }

        [Command("invites", "inv")]
        [Description("View Invite settings for your server.")]
        public async Task<IResult> ViewInvitesAsync()
        {
            var config = await _mediator.Send(new GetGuildModConfig.Request(_context.GuildID.Value));

            var guildResult = await _guilds.GetGuildAsync(_context.GuildID.Value);

            if (!guildResult.IsDefined(out var guild))
                return guildResult;

            var enabled = config.Invites.WhitelistEnabled ? Emojis.EnabledEmoji : Emojis.DisabledEmoji;
            var delete  = config.Invites.DeleteOnMatch ? Emojis.EnabledEmoji : Emojis.DisabledEmoji;
            var action  = config.Invites.WarnOnMatch ? Emojis.EnabledEmoji : Emojis.DisabledEmoji;

            var embed = new Embed
            {
                Colour = Color.MidnightBlue,
                Title  = $"Invite detection for {guild.Name}",
                Description = $"**Enabled:** {enabled}\n"       +
                              $"**Delete Invites:** {delete}\n" +
                              $"**Warn On Invite:** {action}"
            };

            return await _channels.CreateMessageAsync(_context.ChannelID, embeds: new[] { embed });
        }


        [Command("greetings", "welcome")]
        [Description("View greeting settings for your server.")]
        public async Task<IResult> ViewGreetingsAsync
        (
            [Description("The ID of the specific greeting to view. Leave blank to view all greetings.")]
            int? id = null
        )
        {
            var config = await _mediator.Send(new GetGuildConfig.Request(_context.GuildID.Value));

            Embed embed;
            
            if (id is null)
            {
                var greetings = 
                    config
                       .Greetings
                       .Select
                        (g =>
                         {
                             var enabled  = g.Option is GreetingOption.DoNotGreet ? Emojis.DisabledEmoji : Emojis.EnabledEmoji;
                             var role     = g.Option is GreetingOption.GreetOnRole ? $"(<@&{g.MetadataID}>) " : null;
                             var option   = g.Option.Humanize(LetterCasing.Title);
                             var greeting = g.Message.Truncate(50, " [...]");
                             var channel  = $"<#{g.ChannelID}>";
                             
                             return $"{enabled} **`{g.Id}`** ➜ {option} {role}in {channel} \n> {greeting}\n";
                         }
                        );

                embed = new()
                {
                    Colour      = Color.Goldenrod,
                    Title       = $"All greetings ({greetings.Count()})",
                    Description = greetings.Join("\n")
                };
            }
            else
            {
                if (config.Greetings.FirstOrDefault(g => g.Id == id) is not {} greeting)
                    return await _channels.CreateMessageAsync(_context.ChannelID, "I don't see a greeting with that ID!");
                
                embed = new()
                {
                    Colour      = Color.Goldenrod,
                    Title       = $"Greeting {id}",
                    Description = greeting.Message
                };
            }

            return await _channels.CreateMessageAsync(_context.ChannelID, embeds: new[] { embed });
        }
    }

    [Group("edit", "e")]
    [Description("Edit the settings for your server.")]
    public class EditConfigCommands : CommandGroup
    {
        private readonly IMediator              _mediator;
        private readonly MessageContext         _context;
        private readonly IDiscordRestGuildAPI   _guilds;
        private readonly IDiscordRestUserAPI    _users;
        private readonly IDiscordRestInviteAPI  _invites;
        private readonly IDiscordRestChannelAPI _channels;
        private readonly IDiscordRestWebhookAPI _webhooks;
        public EditConfigCommands
        (
            IMediator              mediator,
            MessageContext         context,
            IDiscordRestGuildAPI   guilds,
            IDiscordRestUserAPI    users,
            IDiscordRestChannelAPI channels,
            IDiscordRestInviteAPI  invites,
            IDiscordRestWebhookAPI webhooks
        )
        {
            _mediator      = mediator;
            _context       = context;
            _guilds        = guilds;
            _users         = users;
            _channels      = channels;
            _invites       = invites;
            _webhooks = webhooks;
        }

        [Command("phishing")]
        [SuppressMessage("ReSharper", "RedundantBlankLines", Justification = "Readability")]
        [Description("Edit the settings for phishing detection.")]
        public async Task<IResult> PhishingAsync
        (
            [Option("enabled")]
            [Description("Whether phishing detection should be enabled.")]
            bool?   enabled = null,
            
            [Option("action")]
            [Description("What action to take when phishing is detected. (kick, ban, or mute)")]
            string? action  = null,
            
            [Switch("preserve")]
            [Description("Whether to preserve the message that contains phishing. Not recommended.")]
            bool   preserve  = false
        )
        {
            if (action is not null and not ("kick" or "ban" or "mute"))
                return await _channels.CreateMessageAsync(_context.ChannelID, "Invalid action. Valid actions are: kick, ban, and mute.");

            InfractionType? parsedAction = action switch
            {
                "kick" => InfractionType.Kick,
                "ban"  => InfractionType.Ban,
                "mute" => InfractionType.Mute,
                null   => null,
                _      => throw new ArgumentOutOfRangeException(nameof(action), action, "Impossible condition.")
            };

            var config = await _mediator.Send(new GetGuildModConfig.Request(_context.GuildID.Value));
            
            if (action is not null)
                config!.NamedInfractionSteps[AutoModConstants.PhishingLinkDetected] = new() { Type = parsedAction.Value };

            await _mediator.Send(new UpdateGuildModConfig.Request(_context.GuildID.Value)
            {
                DetectPhishingLinks  = enabled ?? default(Optional<bool>),
                DeletePhishingLinks  = !preserve,
                NamedInfractionSteps = config.NamedInfractionSteps

            });
            
            return await _channels.CreateReactionAsync(_context.ChannelID, _context!.MessageID, $"_:{Emojis.ConfirmId}");
        }
        
        //TODO: Infraction config (stepped, not named)

        [Command("invites")]
        [Description("Adjust the settings for invite detection.")]
        [SuppressMessage("ReSharper", "RedundantBlankLines", Justification = "Readability")]
        public async Task<IResult> Invites
        (
            [Option('d', "delete")]
            [Description("Whether to delete non-whitelisted invites.")]
            bool? delete = null,
            
            [Option('a', "aggressive")]
            [Description("Whether to use a more aggressive invite detection algorithm.")]
            bool? aggressive = null,
            
            [Option('s', "scan")]
            [Description("Whether the origin of the invite should be scanned prior to actioning against it. " +
                         "This is necessary if the server does not have a vanity invite.")]
            bool? scanOrigin = null,
            
            [Option('w', "warn")]
            [Description("Whether to warn the user when an invite is detected.")]
            bool? warnOnMatch = null
        )
        {
            if ((delete ?? aggressive ?? scanOrigin ?? warnOnMatch) is null)
                return await _channels.CreateMessageAsync(_context.ChannelID, "You must specify at least one option.");
            
            await _mediator.Send(new UpdateGuildModConfig.Request(_context.GuildID.Value)
            {
                DeleteOnMatchedInvite = delete      ?? default(Optional<bool>),
                UseAggressiveRegex    = aggressive  ?? default(Optional<bool>),
                ScanInvites           = scanOrigin  ?? default(Optional<bool>),
                WarnOnMatchedInvite   = warnOnMatch ?? default(Optional<bool>)
            });
            
            return await _channels.CreateReactionAsync(_context.ChannelID, _context!.MessageID, $"_:{Emojis.ConfirmId}");
        }

        #region Logging
        
        [Command("logging")]
        [Description("Adjust the settings for logging. \n"                    +
                     "If removing options, true or false can be specified.\n" +
                     "If a channel is already specified for the action, it will be overridden with the new one.")]
        public async Task<IResult> LoggingAsync
        (
            [Option('j', "joins")] 
            [Description("Whether to log when a user joins. ")]
            bool? logJoins = null,

            [Option('l', "leaves")] 
            [Description("Whether to log when a user leaves.")]
            bool? logLeaves = null,

            [Option('d', "deletes")] 
            [Description("Whether to log message deletes.")]
            bool? logDeletes = null,

            [Option('e', "edits")] 
            [Description("Whether to log message edits.")]
            bool? logEdits = null,

            [Option('w', "webhook")] 
            [Description("Whether to log to a webhook.")]
            bool? webhook = null,

            [Switch('r', "remove")] 
            [Description("Removes specified options from the logging settings.")]
            bool remove = false,
            
            [Option('c', "channel")]
            [Description("The channel to log to. This is required if not removing options.")]
            IChannel? channel = null
        )
        {
            if (!remove && channel is null && webhook is null)
            {
                await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, $"_:{Emojis.DeclineId}");
                return await _channels.CreateMessageAsync(_context.ChannelID, "`--channel` or `--webhook` is must be specified.");
            }
            
            var modConfig = await _mediator.Send(new GetGuildModConfig.Request(_context.GuildID.Value));

            var loggingConfig = modConfig.Logging;
            
            if (remove)
            {
                if (logJoins is not null)
                {
                    loggingConfig.LogMemberJoins = false;
                    loggingConfig.MemberJoins    = null;
                }
                
                if (logLeaves is not null)
                {
                    loggingConfig.LogMemberLeaves = false;
                    loggingConfig.MemberLeaves    = null;
                }
                
                if (logDeletes is not null)
                {
                    loggingConfig.LogMessageDeletes = false;
                    loggingConfig.MessageDeletes    = null;
                }

                if (logEdits is not null)
                {
                    loggingConfig.LogMessageEdits = false;
                    loggingConfig.MessageEdits    = null;
                }
                
                await _mediator.Send(new UpdateGuildModConfig.Request(_context.GuildID.Value)
                {
                    LoggingConfig = loggingConfig
                });

                return await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, $"_:{Emojis.ConfirmId}");
            }

            if (channel is not null)
            {
                if (logJoins is true)
                {
                    loggingConfig.LogMemberJoins = true;

                    var lwt = loggingConfig.MemberJoins?.WebhookToken;
                    var lwi = loggingConfig.MemberJoins?.WebhookID;
                    
                    loggingConfig.MemberJoins    = new()
                    {
                        ChannelID = channel.ID,
                        GuildID = _context.GuildID.Value,
                        WebhookToken = lwt ?? "",
                        WebhookID = lwi ?? default(Snowflake)
                    };
                }
                
                if (logLeaves is true)
                {
                    loggingConfig.LogMemberLeaves = true;

                    var lwt = loggingConfig.MemberLeaves?.WebhookToken;
                    var lwi = loggingConfig.MemberLeaves?.WebhookID;
                    
                    loggingConfig.MemberLeaves    = new()
                    {
                        ChannelID = channel.ID,
                        GuildID = _context.GuildID.Value,
                        WebhookToken = lwt ?? "",
                        WebhookID = lwi ?? default(Snowflake)
                    };
                }
                
                if (logDeletes is true)
                {
                    loggingConfig.LogMessageDeletes = true;

                    var lwt = loggingConfig.MessageDeletes?.WebhookToken;
                    var lwi = loggingConfig.MessageDeletes?.WebhookID;
                    
                    loggingConfig.MessageDeletes    = new()
                    {
                        ChannelID = channel.ID,
                        GuildID = _context.GuildID.Value,
                        WebhookToken = lwt ?? "",
                        WebhookID = lwi ?? default(Snowflake)
                    };
                }
                
                if (logEdits is true)
                {
                    loggingConfig.LogMessageEdits = true;

                    var lwt = loggingConfig.MessageEdits?.WebhookToken;
                    var lwi = loggingConfig.MessageEdits?.WebhookID;
                    
                    loggingConfig.MessageEdits = new()
                    {
                        ChannelID = channel.ID,
                        GuildID = _context.GuildID.Value,
                        WebhookToken = lwt ?? "",
                        WebhookID = lwi ?? default(Snowflake)
                    };
                }
            }


            if (webhook is true && channel is not null)
            {
                var success = true;
                
                if (logJoins is true)
                {
                    var joinWebhook = await TryCreateWebhookAsync(channel.ID);

                    if (joinWebhook.IsDefined(out var jw))
                        loggingConfig.MemberJoins = new()
                        {
                            WebhookID    = jw.ID,
                            ChannelID    = channel.ID,
                            WebhookToken = jw.Token.Value,
                            GuildID      = _context.GuildID.Value
                        };
                    else success = false;
                }
                
                if (logLeaves is true)
                {
                    var leaveWebhook = await TryCreateWebhookAsync(channel.ID);

                    if (leaveWebhook.IsDefined(out var lw))
                        loggingConfig.MemberLeaves = new()
                        {
                            WebhookID    = lw.ID,
                            ChannelID    = channel.ID,
                            WebhookToken = lw.Token.Value,
                            GuildID      = _context.GuildID.Value
                        };
                    else success = false;
                }
                
                if (logDeletes is true)
                {
                    var deleteWebhook = await TryCreateWebhookAsync(channel.ID);

                    if (deleteWebhook.IsDefined(out var dw))
                        loggingConfig.MessageDeletes = new()
                        {
                            WebhookID    = dw.ID,
                            ChannelID    = channel.ID,
                            WebhookToken = dw.Token.Value,
                            GuildID      = _context.GuildID.Value
                        };
                    else success = false;
                }
                
                if (logEdits is true)
                {
                    var editWebhook = await TryCreateWebhookAsync(channel.ID);

                    if (editWebhook.IsDefined(out var ew))
                        loggingConfig.MessageEdits = new()
                        {
                            WebhookID    = ew.ID,
                            ChannelID    = channel.ID,
                            WebhookToken = ew.Token.Value,
                            GuildID      = _context.GuildID.Value
                        };
                    else success = false;
                }

                if (!success)
                {
                    await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, $"_:{Emojis.DeclineId}");

                    return await _channels.CreateMessageAsync(_context.ChannelID, "I couldn't create webhooks in that channel. Check the channel settings to ensure I have `Manage Webhooks` please!");
                }

                await _mediator.Send(new UpdateGuildModConfig.Request(_context.GuildID.Value)
                {
                    LoggingConfig = loggingConfig
                });
            }
            
            return await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, $"_:{Emojis.ConfirmEmoji}");
        }
        
        #endregion
        
        private async Task<Result<IWebhook>> TryCreateWebhookAsync(Snowflake channelID)
        {
            var selfResult = await _users.GetCurrentUserAsync();
            
            if (!selfResult.IsSuccess)
                return Result<IWebhook>.FromError(selfResult.Error);
            
            var webhooks = await _webhooks.GetChannelWebhooksAsync(channelID);

            if (!webhooks.IsSuccess)
                return Result<IWebhook>.FromError(webhooks.Error);

            //Webhook tokens are always returned (if you have permission), so we need to check if the wh is owned by us.
            var webhook = webhooks.Entity.FirstOrDefault
                (
                 wh =>
                     wh.Type is WebhookType.Incoming &&
                     wh.User.IsDefined(out var user) &&
                     user.ID == selfResult.Entity.ID
                );

            //Return the webhook if it already exists.
            if (webhook is not null)
                return Result<IWebhook>.FromSuccess(webhook);

            var createResult = await _webhooks.CreateWebhookAsync(channelID, "Silk! Logging", default);

            return createResult;
        }

        #region  Invite Whitelist

        [Command("invite-whitelist", "iw")]
        [Description("Control the whitelisting of invites!")]
        [SuppressMessage("ReSharper", "RedundantBlankLines", Justification = "Readability")]
        public async Task<IResult> WhitelistAsync
        (
            [Option('a', "add")]
            [Description("Add one or more invite(s) to the whitelist. Guild IDs can also be specified!")]
            OneOf<string, Snowflake>[] add,
            
            [Option('r', "remove")]
            [Description("Remove one or more invite(s) from the whitelist. Guild IDs can also be specified!")]
            OneOf<string, Snowflake>[] remove,
            
            [Switch('c', "clear")]
            [Description("Clear the whitelist. For convenience, a dump of all current whitelisted invites will be sent to the channel.")]
            bool clear = false,
            
            [Option("active")]
            [Description("Whether the whitelist is active.")]
            bool? active = null
        )
        {
            var modConfig = await _mediator.Send(new GetGuildModConfig.Request(_context.GuildID.Value));
            
            if (clear)
            {
                if (!modConfig.Invites.Whitelist.Any())
                {
                    await _channels.CreateMessageAsync(_context.ChannelID, "There are no invites to clear!");
                    return Result.FromSuccess();
                }

                var inviteString = modConfig.Invites.Whitelist.Select(r => r.VanityURL).Join(" ");

                await _mediator.Send(new UpdateGuildModConfig.Request(_context.GuildID.Value) {AllowedInvites = Array.Empty<InviteEntity>().ToList()});
                
                await _channels.CreateMessageAsync(_context.ChannelID, $"Here's a dump of the whitelist prior to clearing! \n{inviteString}");

                return Result.FromSuccess();
            }

            var addedInvites  = new List<string>();
            var failedAdds = new List<string>();
            
            var removedInvites = new List<string>();
            var failedRemoves  = new List<string>();
            
            
            foreach (var added in add)
            {
                if (added.TryPickT0(out var inviteString, out var guildID))
                {
                    if (modConfig.Invites.Whitelist.Any(iv => iv.VanityURL == inviteString))
                    {
                        failedAdds.Add($"`{inviteString,-15}{"(already whitelisted)`",30}");
                        continue;
                    }

                    var inviteResult = await _invites.GetInviteAsync(inviteString);

                    if (!inviteResult.IsDefined(out var inv))
                    {
                        failedAdds.Add($"`{inviteString,-15}{"(invalid invite)`",30}");
                        continue;
                    }

                    if (inv.Guild.IsDefined(out var inviteGuild) && inviteGuild.ID.Value == _context.GuildID.Value)
                    {
                        failedAdds.Add($"`{inviteString,-15}{"(invite is for this server)`",30}");
                        continue;
                    }

                    if (inv.ExpiresAt.IsDefined())
                    {
                        failedAdds.Add($"`{inviteString,-15}{"(invite is temporary)`",30}");
                        continue;
                    }
                    
                    modConfig.Invites.Whitelist.Add(new () { VanityURL = inviteString, GuildId = _context.GuildID.Value});
                    addedInvites.Add($"`{inviteString,-44}`");;
                }
                else
                {
                    if (guildID == _context.GuildID.Value)
                    {
                        failedAdds.Add($"`{guildID.ToString(),-15}{"(invite is for this server)`",30}");
                        continue;
                    }
                    
                    if (modConfig.Invites.Whitelist.Any(iv => iv.InviteGuildId == guildID))
                    {
                        failedAdds.Add($"`{guildID.ToString(),-15}{"(already whitelisted)`",30}");
                        continue;
                    }
                    
                    modConfig.Invites.Whitelist.Add(new() { GuildId = _context.GuildID.Value, InviteGuildId = guildID });
                    
                    addedInvites.Add($"`{guildID,-44}`");
                }
            }

            foreach (var removed in remove)
            {
                if (!modConfig.Invites.Whitelist.Any())
                {
                    failedRemoves.Add("The whitelist is empty!".PadRight(34));
                    break;
                }
                
                if (removed.TryPickT0(out var inviteString, out var guildID))
                {
                    inviteString = Regex.Replace(inviteString, @"(https?:\/\/discord\.gg\/)?(?<invite>[A-z0-9_-]+)", "${invite}");
                    
                    if (modConfig.Invites.Whitelist.All(iv => iv.VanityURL != inviteString))
                    {
                        failedRemoves.Add($"`{inviteString,-15}{"(not whitelisted)`",30}");
                        continue;
                    }
                    
                    modConfig.Invites.Whitelist.RemoveAll(iv => iv.VanityURL == inviteString);
                    removedInvites.Add($"`{inviteString,-44}`");
                }
                else
                {
                    if (modConfig.Invites.Whitelist.All(iv => iv.InviteGuildId != guildID))
                    {
                        failedRemoves.Add($"`{guildID.ToString(),-15}{"(not whitelisted)`",30}");
                        continue;
                    }
                    
                    modConfig.Invites.Whitelist.RemoveAll(iv => iv.InviteGuildId == guildID);
                    removedInvites.Add($"`{guildID,-44}`");
                }
            }
            
            var messageBuilder = new StringBuilder();

            if (addedInvites.Any())
            {
                messageBuilder.AppendLine($"Added {addedInvites.Count} invites to the whitelist:");
                
                foreach (var invite in addedInvites)
                    messageBuilder.AppendLine(invite);

                messageBuilder.AppendLine();
            }

            if (removedInvites.Any())
            {
                messageBuilder.AppendLine($"Removed {removedInvites.Count} invites from the whitelist:");
                
                foreach (var invite in removedInvites)
                    messageBuilder.AppendLine(invite);

                messageBuilder.AppendLine();
            }
            
            if (failedAdds.Any())
            {
                messageBuilder.AppendLine($"Failed to add {failedAdds.Count} invites from the whitelist:");
                
                foreach (var invite in failedAdds)
                    messageBuilder.AppendLine(invite);

                messageBuilder.AppendLine();
            }

            if (failedRemoves.Any())
            {
                messageBuilder.AppendLine($"Failed to remove {failedRemoves.Count} invites to the whitelist:");
                
                foreach (var invite in failedRemoves)
                    messageBuilder.AppendLine(invite);
            }
            
            await _mediator.Send(new UpdateGuildModConfig.Request(_context.GuildID.Value)
            {
                AllowedInvites = modConfig.Invites.Whitelist,
                BlacklistInvites = active ?? default
            });

            
            if (messageBuilder.Length > 0)
                return await _channels.CreateMessageAsync(_context.ChannelID, messageBuilder.ToString());
            
            
            return await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, $"_:{Emojis.ConfirmId}");
        }

        #endregion

        
        [Command("mute")]
        [SuppressMessage("ReSharper", "RedundantBlankLines", Justification = "Readability")]
        [Description("Adjust the configured mute role, or setup native mutes (powered by Discord's Timeout feature).")]
        public async Task<IResult> MuteAsync
        (
            [Description("The role to mute users with.")]
            IRole? mute,
            
            [Option("native")]                            
            [Description("Whether to use the native mute functionality. This requires the `Timeout Members` permission.")]
            bool? useNativeMute = null
            //It's worth noting that there WAS an option here to have Silk automatically configure the role,
            // but between ratelimits and the fact that permissions suck, it was removed.
        )
        {
            
            var selfResult = await _guilds.GetCurrentGuildMemberAsync(_users, _context.GuildID.Value);
                
            if (!selfResult.IsDefined(out var self))
                return selfResult;
                
            var guildRoles = await _guilds.GetGuildRolesAsync(_context.GuildID.Value);
                
            if (!guildRoles.IsDefined(out var roles))
                return guildRoles;

            var selfRoles = roles.Where(r => self.Roles.Contains(r.ID)).ToArray();

            var selfPerms = DiscordPermissionSet.ComputePermissions(self.User.Value.ID, roles.First(r => r.ID == _context.GuildID), selfRoles);
            
            if (useNativeMute is not null && useNativeMute.Value && !selfPerms.HasPermission(DiscordPermission.ModerateMembers)) 
                return await _channels.CreateMessageAsync(_context.ChannelID, "I don't have permission to timeout members!");
            
            if (mute is not null)
            {
                if (!selfPerms.HasPermission(DiscordPermission.ManageRoles))
                    return await _channels.CreateMessageAsync(_context.ChannelID, "I don't have permission to assign roles!");
                
                if (mute.ID == _context.GuildID)
                    return await _channels.CreateMessageAsync(_context.ChannelID, "You can't assign the everyone role as a mute role!");
                
                if (mute.Position >= selfRoles.Max(r => r.Position))
                    return await _channels.CreateMessageAsync(_context.ChannelID, "This role is above my highest role! I can't assign it.");
                
                if (mute.Permissions.HasPermission(DiscordPermission.SendMessages))
                    return await _channels.CreateMessageAsync(_context.ChannelID, "This role can send messages. It's not a good idea to assign it to a mute role.");
            }
            
            await _mediator.Send(new UpdateGuildModConfig.Request(_context.GuildID.Value)
            {
                MuteRoleID = mute?.ID ?? default(Optional<Snowflake>),
                UseNativeMute = useNativeMute ?? default(Optional<bool>)
            });
            
            return await _channels.CreateReactionAsync(_context.ChannelID, _context!.MessageID, $"_:{Emojis.ConfirmId}");
        }



        [Group("greetings", "greeting", "welcome")]
        public class GreetingCommands : CommandGroup
        {
            private readonly IMediator              _mediator;
            private readonly MessageContext        _context;
            private readonly IDiscordRestChannelAPI _channels;

            public enum GreetOption
            {
                Ignore = GreetingOption.DoNotGreet,
                Join    = GreetingOption.GreetOnJoin, 
                Role    = GreetingOption.GreetOnRole,
                //Screen  = GreetingOption.GreetOnScreening
            }

            public GreetingCommands
            (
                IMediator mediator,
                MessageContext context,
                IDiscordRestChannelAPI channels
            )
            {
                _mediator = mediator;
                _context  = context;
                _channels = channels;
            }

            [Command("add")]
            [Description("Add a greeting message to a channel.")]
            [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
            public async Task<IResult> AddAsync
            (
                [Description("The channel to add the greeting to.")]
                IChannel channel,

                [Description("When to greet the member. Available options are `join` and `role`.")]
                GreetOption option,

                [Greedy]
                [Description(
                                "The welcome message to send. \n"                                  +
                                "The following subsitutions are supported:\n"                      +
                                "`{s}` - The name of the server.\n"                              +
                                "`{u}` - The username of the user who joined.\n"                   +
                                "`{@u}` - The mention (@user) of the user who joined.\n\n"           +
                                "Greetings larger than 2000 characters will be placed an embed.\n" +
                                "Embeded greetings do not generate pings for mentioned users/roles."
                            )]
                string greeting,

                [Option("role")]
                [Description("The role to check for. \n" +
                             "This can be an ID (`123456789012345678`), or a mention (`@Role`).")]
                IRole? role = null
            )
            {
                if (option is GreetOption.Role && role is null)
                {
                    await _channels.CreateMessageAsync(_context.ChannelID, "You must specify a role to check for!");

                    return await _channels.CreateReactionAsync(_context.ChannelID, _context!.MessageID, $"_:{Emojis.DeclineId}");
                }
                
                var config = await _mediator.Send(new GetGuildConfig.Request(_context.GuildID.Value));

                if (config.Greetings.FirstOrDefault(g => g.ChannelID == channel.ID) is {} existingGreeting)
                {
                    await _channels.CreateMessageAsync
                        (
                         _context.ChannelID,
                         $"There appears to already be a greeting set up for that channel! (ID `{existingGreeting.Id}`)\n\n" +
                         "Consider updating or deleting that greeting instead!"
                        );
                    
                    return await _channels.CreateReactionAsync(_context.ChannelID, _context!.MessageID, $"_:{Emojis.DeclineId}");
                }

                var greetingEntity = new GuildGreetingEntity
                {
                    Message = greeting,
                    ChannelID = channel.ID,
                    MetadataID = role?.ID,
                    Option = (GreetingOption)option
                };
                
                config.Greetings.Add(greetingEntity);

                await _mediator.Send(new UpdateGuildConfig.Request(_context.GuildID.Value, config.Greetings));

                var message = $"Created greeting with ID `{greetingEntity.Id}`\n\n";
                
                if (greeting.Length > 2000)
                    message += $"Be warned! This greeting is larger than 2000 characters ({greeting.Length}), and will be placed an embed.";

                await _channels.CreateMessageAsync(_context.ChannelID, message);
                
                return await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, $"_:{Emojis.ConfirmId}");
            }

            [Command("update")]
            [Description("Updates an existing greeting.")]
            public async Task<IResult> UpdateGreetingAsync
            (
                [Description("The ID of the greeting to update.")]
                int GreetingID,

                [Option("on")]
                [Description("When to greet the member (`join` or `role`).")]
                GreetOption? option = null,
                
                [Option("role")]
                [Description("The role to check for when greeting")]
                IRole? role = null,
                
                [Option("channel")]
                [Description("The new channel to send greetings to")]
                IChannel? channel = null,
                
                [Greedy]
                [Option("greeting")]
                [Description("The new greeting")]
                string? greeting = null
            )
            {
                var config = await _mediator.Send(new GetGuildConfig.Request(_context.GuildID.Value));
                
                var greetingEntity = config.Greetings.FirstOrDefault(x => x.Id == GreetingID);
                
                if (greetingEntity is null)
                {
                    await _channels.CreateMessageAsync(_context.ChannelID, "Could not find a greeting with that ID!");

                    return await _channels.CreateReactionAsync(_context.ChannelID, _context!.MessageID, $"_:{Emojis.DeclineId}");
                }

                if (option is GreetOption.Role && role is null)
                {
                    await _channels.CreateMessageAsync(_context.ChannelID, "You must specify a role to check for!");
                    
                    return await _channels.CreateReactionAsync(_context.ChannelID, _context!.MessageID, $"_:{Emojis.DeclineId}");
                }
                
                greetingEntity.ChannelID  = channel?.ID ?? greetingEntity.ChannelID;
                greetingEntity.MetadataID = role?.ID    ?? greetingEntity.MetadataID;
                greetingEntity.Message    = greeting    ?? greetingEntity.Message;
                
                if (option is not null)
                    greetingEntity.Option = (GreetingOption)option;
                
                await _mediator.Send(new UpdateGuildConfig.Request(_context.GuildID.Value, config.Greetings));
                
                var message = $"Updated greeting with ID `{greetingEntity.Id}`\n\n";
                
                if (greeting?.Length > 2000)
                    message += $"Be warned! This greeting is larger than 2000 characters ({greeting.Length}), and will be placed an embed.";
                
                await _channels.CreateMessageAsync(_context.ChannelID, message);
                
                return await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, $"_:{Emojis.ConfirmId}");
            }

            [Command("delete")]
            [Description("Deletes an existing greeting.")]
            public async Task<IResult> Delete
            (
                [Description("The ID of the greeting to delete.")]
                int GreetingID
            )
            {
                var config = await _mediator.Send(new GetGuildConfig.Request(_context.GuildID.Value));
                
                var greetingEntity = config.Greetings.FirstOrDefault(x => x.Id == GreetingID);
                
                if (greetingEntity is null)
                {
                    await _channels.CreateMessageAsync(_context.ChannelID, "I can't seem to find a greeting with that ID!");

                    return await _channels.CreateReactionAsync(_context.ChannelID, _context!.MessageID, $"_:{Emojis.DeclineId}");
                }
                
                config.Greetings.Remove(greetingEntity);
                
                await _mediator.Send(new UpdateGuildConfig.Request(_context.GuildID.Value, config.Greetings));
                
                await _channels.CreateMessageAsync(_context.ChannelID, $"Deleted greeting with ID `{greetingEntity.Id}`");
                
                return await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, $"_:{Emojis.ConfirmId}");
            }
        }
    }
}