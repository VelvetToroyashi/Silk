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

namespace Silk.Commands;

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
        
        return await _channels.CreateMessageAsync(_context.ChannelID, "Done! Changes should take effect immediately.");
    }
    
    [Group("view", "v")]
    [Description("View the settings for your server.")]
    public class ViewConfigCommands : CommandGroup
    {
        private readonly IMediator              _mediator;
        private readonly MessageContext        _context;
        private readonly IDiscordRestGuildAPI  _guilds;
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

        [Command("all", "a")]
        [Description("View all settings for your server.")]
        public async Task<IResult> ViewAllAsync()
        {
            var config    = await _mediator.Send(new GetGuildConfig.Request(_context.GuildID.Value));
            var modConfig = await _mediator.Send(new GetGuildModConfig.Request(_context.GuildID.Value));

            var guildResult = await _guilds.GetGuildAsync(_context.GuildID.Value);

            if (!guildResult.IsDefined(out var guild))
                return guildResult;
            
            var contentBuilder = new StringBuilder();

            contentBuilder
               .Clear()
               .AppendLine("**General Config:**")
               .AppendLine("__Greetings__ | `config edit greetings`")
               .AppendLine($"> Configured Greetings: {config.Greetings.Count}")
               .AppendLine()
               .AppendLine("**Moderation Config:**")
               .AppendLine()
               .AppendLine("__Logging__ | Soon:tm:")
               .AppendLine($"> Log members joining: <:_:{(modConfig.LoggingConfig.LogMemberJoins ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine($"> Log members leaving: <:_:{(modConfig.LoggingConfig.LogMemberLeaves ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine($"> Log message edits: <:_:{(modConfig.LoggingConfig.LogMessageEdits ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine($"> Log message deletes: <:_:{(modConfig.LoggingConfig.LogMessageDeletes ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine()
               .AppendLine("__Invites__ | `config edit invites`, `config edit invite-whitelist`")
               .AppendLine($"> Scan invite: <:_:{(modConfig.ScanInviteOrigin ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine($"> Warn on invite: <:_:{(modConfig.InfractOnMatchedInvite ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine($"> Delete matched invite: <:_:{(modConfig.DeleteMessageOnMatchedInvite ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine($@"> Use aggressive invite matching: <:_:{(modConfig.UseAggressiveRegex ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine($"> Allowed invites: {(modConfig.AllowedInvites?.Count is 0 ? "None" : $"{modConfig.AllowedInvites.Count} allowed invites [See `config view invites`]")}")
               .AppendLine("Aggressive pattern matching regex:")
               .AppendLine(@"`disc((ord)?(((app)?\.com\/invite)|(\.gg)))\/([A-z0-9-_]{2,})`")
               .AppendLine()
               .AppendLine("__Infractions__ | `config edit infractions`")
               .AppendLine($"> Mute role: {(modConfig.MuteRoleID.Value is 0 ? "Not set" : $"<@&{modConfig.MuteRoleID}>")}")
               .AppendLine($"> Auto-escalate auto-mod infractions: <:_:{(modConfig.ProgressiveStriking ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine($"> Infraction steps: {(modConfig.InfractionSteps?.Count is var dictCount and not 0 ? $"{dictCount} steps [See `config view infractions`]" : "Not configured")}")
               .AppendLine($"> Infraction steps (named): {((modConfig.NamedInfractionSteps?.Count ?? 0) is var infNameCount and not 0 ? $"{infNameCount} steps [See `config view infractions`]" : "Not configured")}")
               .AppendLine()
               .AppendLine("__Anti-Phishing__ **(New!)** | `config edit phishing`")
               .AppendLine($"> Anti-Phishing enabled: <:_:{(modConfig.DetectPhishingLinks ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine($"> Delete Phishing Links: <:_:{(modConfig.DeletePhishingLinks ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine($"> Phishing detection action: {(modConfig.NamedInfractionSteps!.TryGetValue(AutoModConstants.PhishingLinkDetected, out var action) ? action.Type : "Not configured")}");

            var embed = new Embed
            {
                Title       = $"Config for {guild.Name}!",
                Colour      = Color.MidnightBlue,
                Description = contentBuilder.ToString()
            };
            
            return await _channels.CreateMessageAsync(_context.ChannelID, embeds: new[] {embed});
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

            var loggingConfig = modConfig.LoggingConfig;
            
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
                if (!modConfig!.AllowedInvites.Any())
                {
                    await _channels.CreateMessageAsync(_context.ChannelID, "There are no invites to clear!");
                    return Result.FromSuccess();
                }

                var inviteString = modConfig.AllowedInvites.Select(r => r.VanityURL).Join(" ");

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
                    if (modConfig!.AllowedInvites.Any(iv => iv.VanityURL == inviteString))
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
                    
                    modConfig.AllowedInvites.Add(new () { VanityURL = inviteString, GuildId = _context.GuildID.Value});
                    addedInvites.Add($"`{inviteString,-44}`");;
                }
                else
                {
                    if (guildID == _context.GuildID.Value)
                    {
                        failedAdds.Add($"`{guildID.ToString(),-15}{"(invite is for this server)`",30}");
                        continue;
                    }
                    
                    if (modConfig!.AllowedInvites.Any(iv => iv.InviteGuildId == guildID))
                    {
                        failedAdds.Add($"`{guildID.ToString(),-15}{"(already whitelisted)`",30}");
                        continue;
                    }
                    
                    modConfig.AllowedInvites.Add(new() { GuildId = _context.GuildID.Value, InviteGuildId = guildID });
                    
                    addedInvites.Add($"`{guildID,-44}`");
                }
            }

            foreach (var removed in remove)
            {
                if (!modConfig!.AllowedInvites.Any())
                {
                    failedRemoves.Add("The whitelist is empty!".PadRight(34));
                    break;
                }
                
                if (removed.TryPickT0(out var inviteString, out var guildID))
                {
                    inviteString = Regex.Replace(inviteString, @"(https?:\/\/discord\.gg\/)?(?<invite>[A-z0-9_-]+)", "${invite}");
                    
                    if (modConfig!.AllowedInvites.All(iv => iv.VanityURL != inviteString))
                    {
                        failedRemoves.Add($"`{inviteString,-15}{"(not whitelisted)`",30}");
                        continue;
                    }
                    
                    modConfig.AllowedInvites.RemoveAll(iv => iv.VanityURL == inviteString);
                    removedInvites.Add($"`{inviteString,-44}`");
                }
                else
                {
                    if (modConfig!.AllowedInvites.All(iv => iv.InviteGuildId != guildID))
                    {
                        failedRemoves.Add($"`{guildID.ToString(),-15}{"(not whitelisted)`",30}");
                        continue;
                    }
                    
                    modConfig.AllowedInvites.RemoveAll(iv => iv.InviteGuildId == guildID);
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
                AllowedInvites = modConfig.AllowedInvites,
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

/*using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Humanizer;
using MediatR;
using NpgsqlTypes;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Data.MediatR.Guilds.Config;
using Silk.Services.Interfaces;
using Silk.Utilities;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;
using Silk.Shared.Constants;

namespace Silk.Commands;

[RequireGuild]
[Group("config")]

[Description("View and edit configuration for the current guild.")]
public class ConfigModule : BaseCommandModule
{
    private readonly ICacheUpdaterService _updater;
    public ConfigModule(ICacheUpdaterService updater) => _updater = updater;

    [Command]
    [Description("Reloads the config from the database. May temporarily slow down response time. (Configs are automatically reloaded every 10 minutes!)")]
    public async Task Reload(CommandContext ctx)
    {
        bool res = await EditConfigModule.GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

        if (!res) return;

        _updater.UpdateGuild(ctx.Guild.Id);
    }

    // Wrapper that points to config view //
    [GroupCommand]
    public Task Default(CommandContext ctx) => ctx.CommandsNext.ExecuteCommandAsync(ctx.CommandsNext.CreateContext(ctx.Message, ctx.Prefix, ctx.CommandsNext.RegisteredCommands["config view"]));

    [Group("view")]
    [Description("View the current config, or specify a sub-command to see detailed information.")]
    public sealed class ViewConfigModule : BaseCommandModule
    {
        private readonly IMediator _mediator;
        public ViewConfigModule(IMediator mediator) => _mediator = mediator;

        private string GetCountString(int count) => count is 0 ? "Not set/enabled" : count.ToString();

        [GroupCommand]
        
        [Description("View the current config.")]
        public async Task View(CommandContext ctx)
        {
            GuildConfigEntity?    config    = await _mediator.Send(new GetGuildConfigRequest(ctx.Guild.Id));
            GuildModConfigEntity? modConfig = await _mediator.Send(new GetGuildModConfigRequest(ctx.Guild.Id));

            var embed          = new DiscordEmbedBuilder();
            var contentBuilder = new StringBuilder();

            contentBuilder
               .Clear()
               .AppendLine("**General Config:**")
               .AppendLine("__Greeting:__")
               .AppendLine($"> Option: {config.GreetingOption.Humanize()}")
               .AppendLine($"> Greeting channel {(config.GreetingOption is GreetingOption.DoNotGreet ? "N/A" : $"<#{config.GreetingChannel}>")}")
               .AppendLine($"> Greeting text: {(config.GreetingOption is GreetingOption.DoNotGreet ? "N/A" : $"[See {ctx.Prefix}config view greeting]")}")
               .AppendLine()
               .AppendLine()
               .AppendLine("**Moderation Config:**")
               .AppendLine()
               .AppendLine("__Logging:__")
               .AppendLine($"> Channel: {(modConfig.LoggingChannel is var channel and not 0 ? $"<#{channel}>" : "Not set")}")
               .AppendLine($"> Log members joining: <:_:{(modConfig.LogMemberJoins ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine($"> Log members leaving: <:_:{(modConfig.LogMemberLeaves ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine($"> Log message edits/deletions: <:_:{(modConfig.LogMessageChanges ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine()
               .AppendLine($"Max role mentions: {GetCountString(modConfig.MaxRoleMentions)}")
               .AppendLine($"Max user mentions: {GetCountString(modConfig.MaxUserMentions)}")
               .AppendLine()
               .AppendLine("__Invites:__")
               .AppendLine($"> Scan invite: <:_:{(modConfig.ScanInviteOrigin ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine($"> Warn on invite: <:_:{(modConfig.InfractOnMatchedInvite ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine($"> Delete matched invite: <:_:{(modConfig.DeleteMessageOnMatchedInvite ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine($@"> Use aggressive invite matching: <:_:{(modConfig.UseAggressiveRegex ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine($"> Allowed invites: {(modConfig.AllowedInvites?.Count is 0 ? "None" : $"{modConfig.AllowedInvites.Count} allowed invites [See {ctx.Prefix}config view invites]")}")
               .AppendLine("Aggressive pattern matching regex:")
               .AppendLine(@"`disc((ord)?(((app)?\.com\/invite)|(\.gg)))\/([A-z0-9-_]{2,})`")
               .AppendLine()
               .AppendLine("__Infractions:__")
               .AppendLine($"> Mute role: {(modConfig.MuteRoleID is 0 ? "Not set" : $"<@&{modConfig.MuteRoleID}>")}")
               .AppendLine($"> Auto-escalate auto-mod infractions: <:_:{(modConfig.ProgressiveStriking ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine($"> Infraction steps: {(modConfig.InfractionSteps?.Count is var dictCount and not 0 ? $"{dictCount} steps [See {ctx.Prefix}config view infractions]" : "Not configured")}")
               .AppendLine($"> Infraction steps (named): {((modConfig.NamedInfractionSteps?.Count ?? 0) is var infNameCount and not 0 ? $"{infNameCount} steps [See {ctx.Prefix}config view infractions]" : "Not configured")}")
               .AppendLine()
               .AppendLine("__Anti-Phishing__ **(Beta)**:")
               .AppendLine($"> Anti-Phishing enabled: <:_:{(modConfig.DetectPhishingLinks ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine($"> Delete Phishing Links: <:_:{(modConfig.DeletePhishingLinks ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine($"> Phishing detection action: {(modConfig.NamedInfractionSteps!.TryGetValue(AutoModConstants.PhishingLinkDetected, out InfractionStepEntity? action) ? action.Type : "Not configured")}");

            embed
               .WithTitle($"Configuration for {ctx.Guild.Name}:")
               .WithColor(DiscordColor.Azure)
               .WithDescription(contentBuilder.ToString());

            await ctx.RespondAsync(embed);
        }

        // Justification for omitting a Log command in the View group:			//
        // The commands below exist because they house complex information		//
        // that would otherwise bloat the main embed to > 4096 characters,		//
        // which is the limit for embed descriptions. Log however only houses	//
        // A few booleans, and thus does not need it's own command in the view	//
        // group.																//

        [Command("automod-options")]
        [Aliases("automodoptions", "amo")]
        [Description("View available auto-mod actions.")]
        public async Task AutoModOptions(CommandContext ctx)
        {
            string? options = AutoModConstants.ActionStrings.Select(o => $"`{o.Key}` Definition: {o.Value}").Join("\n");
            if (options.Length <= 4000)
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder().WithColor(DiscordColor.Azure).WithDescription(options));
            }
            else
            {
                InteractivityExtension? interactivity = ctx.Client.GetInteractivity();
                IEnumerable<Page>?      pages         = interactivity.GeneratePagesInEmbed(options, SplitType.Line, new() { Color = DiscordColor.Azure });
                await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages);
            }
        }

        [Command]
        [Description("View in-depth greeting-related config.")]
        public async Task Greeting(CommandContext ctx)
        {
            var                contentBuilder = new StringBuilder();
            GuildConfigEntity? config         = await _mediator.Send(new GetGuildConfigRequest(ctx.Guild.Id));

            contentBuilder
               .Clear()
               .AppendLine("__**Greeting option:**__")
               .AppendLine($"> Option: {config.GreetingOption.Humanize()}")
               .AppendLine($"> Greeting channel {(config.GreetingOption is GreetingOption.DoNotGreet ? "N/A" : $"<#{config.GreetingChannel}>")}")
               .AppendLine($"> Greeting text: {(config.GreetingOption is GreetingOption.DoNotGreet ? "N/A" : $"\n\n{config.GreetingText}")}")
               .AppendLine($"> Greeting role: {(config.GreetingOption is GreetingOption.GreetOnRole && config.VerificationRole is var role and not 0 ? $"<@&{role}>" : "N/A")}");

            string? explanation = config.GreetingOption switch
            {
                GreetingOption.DoNotGreet       => "I will not greet members at all.",
                GreetingOption.GreetOnJoin      => "I will greet members as soon as they join",
                GreetingOption.GreetOnRole      => "I will greet members when they're given a specific role",
                GreetingOption.GreetOnScreening => "I will greet members when they pass membership screening. Only applicable to community servers.",
                _                               => throw new ArgumentOutOfRangeException()
            };

            contentBuilder
               .AppendLine()
               .AppendLine("**Greeting option explanation:**")
               .AppendLine(explanation);

            DiscordEmbedBuilder? embed = new DiscordEmbedBuilder()
                                        .WithColor(DiscordColor.Azure)
                                        .WithTitle($"Config for {ctx.Guild.Name}")
                                        .WithDescription(contentBuilder.ToString());

            await ctx.RespondAsync(embed);
        }

        [Command]
        [Description("View in-depth invite related config.")]
        public async Task Invites(CommandContext ctx)
        {
            //TODO: config view invites-list
            GuildModConfigEntity? config         = await _mediator.Send(new GetGuildModConfigRequest(ctx.Guild.Id));
            var                   contentBuilder = new StringBuilder();

            contentBuilder
               .Clear()
               .AppendLine("__Invites:__")
               .AppendLine($"> Scan invite: <:_:{(config.ScanInviteOrigin ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine($"> Warn on invite: <:_:{(config.InfractOnMatchedInvite ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine($"> Delete matched invite: <:_:{(config.DeleteMessageOnMatchedInvite ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine($@"> Use aggressive invite matching : <:_:{(config.UseAggressiveRegex ? Emojis.ConfirmId : Emojis.DeclineId)}>")
               .AppendLine()
               .AppendLine($"> Allowed invites: {(config.AllowedInvites.Count is 0 ? "There are no whitelisted invites!" : $"{config.AllowedInvites.Count} allowed invites:")}")
               .AppendLine($"> {config.AllowedInvites.Take(15).Select(inv => $"`{inv.VanityURL}`\n").Join("> ")}");

            if (config.AllowedInvites.Count > 15)
                contentBuilder.AppendLine($"..Plus {config.AllowedInvites.Count - 15} more");

            contentBuilder
               .AppendLine("Aggressive pattern matching are any invites that match this rule:")
               .AppendLine(@"`disc((ord)?(((app)?\.com\/invite)|(\.gg)))\/([A-z0-9-_]{2,})`");

            DiscordEmbedBuilder? embed = new DiscordEmbedBuilder()
                                        .WithTitle($"Configuration for {ctx.Guild.Name}:")
                                        .WithColor(DiscordColor.Azure)
                                        .WithDescription(contentBuilder.ToString());

            await ctx.RespondAsync(embed);
        }

        [Command]
        [Description("View in-depth infraction-related config.")]
        public async Task Infractions(CommandContext ctx)
        {
            GuildModConfigEntity? config = await _mediator.Send(new GetGuildModConfigRequest(ctx.Guild.Id));

            StringBuilder? contentBuilder = new StringBuilder()
                                           .AppendLine("__Infractions:__")
                                           .AppendLine($"> Infraction steps: {(config.InfractionSteps.Count is var dictCount and not 0 ? $"{dictCount} steps" : "Not configured")}")
                                           .AppendLine($"> Infraction steps (named): {((config.NamedInfractionSteps?.Count ?? 0) is var infNameCount and not 0 ? $"{infNameCount} steps" : "Not configured")}")
                                           .AppendLine($"> Auto-escalate auto-mod infractions: <:_:{(config.ProgressiveStriking ? Emojis.ConfirmId : Emojis.DeclineId)}>");

            if (config.InfractionSteps.Any())
            {
                contentBuilder
                   .AppendLine()
                   .AppendLine("Infraction steps:")
                   .AppendLine(config.InfractionSteps.Select((inf, count) => $"` {count + 1} ` strikes -> {inf.Type} {(inf.Duration == NpgsqlTimeSpan.Zero ? "" : $"For {inf.Duration.Time.Humanize()}")}").Join("\n"));
            }

            if (config.NamedInfractionSteps?.Any() ?? false)
            {
                contentBuilder
                   .AppendLine()
                   .AppendLine("Auto-Mod action steps:")
                   .AppendLine(config.NamedInfractionSteps.Select(inf => $"`{inf.Key}` -> {inf.Value.Type} {(inf.Value.Duration == NpgsqlTimeSpan.Zero ? "" : $"For {inf.Value.Duration.Time.Humanize()}")}").Join("\n"));
            }

            DiscordEmbedBuilder? embed = new DiscordEmbedBuilder()
                                        .WithTitle($"Configuration for {ctx.Guild.Name}:")
                                        .WithColor(DiscordColor.Azure)
                                        .WithDescription(contentBuilder.ToString());

            await ctx.RespondAsync(embed);
        }
    }

    [Group("edit")]
    
    [Description("Edit various settings through these commands:")]
    public sealed class EditConfigModule : BaseCommandModule
    {
        // Someone's gonna chew me a new one with this many statics lmao //
        private static readonly DiscordButtonComponent _yesButton = new(ButtonStyle.Success, "confirm action", null, false, new(Emojis.ConfirmId));
        private static readonly DiscordButtonComponent _noButton  = new(ButtonStyle.Danger, "decline action", null, false, new(Emojis.DeclineId));

        private static readonly DiscordButtonComponent _yesButtonDisabled = new DiscordButtonComponent(_yesButton).Disable();
        private static readonly DiscordButtonComponent _noButtonDisabled  = new DiscordButtonComponent(_noButton).Disable();

        private static readonly DiscordInteractionResponseBuilder                    _confirmBuilder = new DiscordInteractionResponseBuilder().WithContent("Alrighty!").AddComponents(_yesButtonDisabled, _noButtonDisabled);
        private static readonly DiscordInteractionResponseBuilder                    _declineBuilder = new DiscordInteractionResponseBuilder().WithContent("Cancelled!").AddComponents(_yesButtonDisabled, _noButtonDisabled);
        private static readonly ConcurrentDictionary<ulong, CancellationTokenSource> _tokens         = new();

        private readonly IMediator _mediator;
        public EditConfigModule(IMediator mediator) => _mediator = mediator;


        [Command]
        
        [Description("Edit the mute role to give to members when muting. If this isn't configured, one will be generated as necessary.")]
        public async Task Mute(CommandContext ctx, DiscordRole role)
        {
            bool notMuteRole       = role.Permissions.HasPermission(Permissions.SendMessages);
            bool canChangeMuteRole = ctx.Guild.CurrentMember.HasPermission(Permissions.ManageRoles);
            bool roleTooHigh       = ctx.Guild.CurrentMember.Roles.Max(r => r.Position) <= role.Position;

            if (notMuteRole)
            {
                string? msg = (canChangeMuteRole, roleTooHigh) switch
                {
                    (true, false)  => "",
                    (true, true)   => "That role is too high and has permission to send messages! Please fix this and try again.",
                    (false, true)  => "I don't have permission to edit this role, and it has permission to send messages! Please fix this and try again.",
                    (false, false) => "This role has permission to send messages, and I can't edit it. Please fix this and try again."
                };

                if (!canChangeMuteRole || roleTooHigh)
                {
                    await ctx.RespondAsync(msg);
                    return;
                }
                await role.ModifyAsync(m => m.Permissions = role.Permissions ^ Permissions.SendMessages);
            }

            EnsureCancellationTokenCancellation(ctx.User.Id);

            bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

            if (!res) return;

            await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { MuteRoleId = role.Id });
        }


        [Command]
        [Aliases("mum", "max_user_mentions", "max-user-mentions")]
        [Description("Edit the maximum amount of unique user mentions allowed in a single message. Set to 0 to disable.")]
        public async Task MaxUserMentions(CommandContext ctx, uint mentions)
        {
            EnsureCancellationTokenCancellation(ctx.User.Id);

            bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

            if (!res) return;

            await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { MaxUserMentions = (int)mentions });
        }

        [Command]
        [Aliases("mrm", "max_role_mentions", "max-role-mentions")]
        [Description("Edit the maximum amount of unique role mentions allowed in a single message. Set to 0 to disable.")]
        public async Task MaxRoleMentions(CommandContext ctx, uint mentions)
        {
            EnsureCancellationTokenCancellation(ctx.User.Id);

            bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

            if (!res) return;

            await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { MaxRoleMentions = (int)mentions });
        }


        [Command]
        [Aliases("welcome")]
        [Description("Edit whether or not I greet members\nOptions: \n\n`role` -> greet on role, \n`join` -> greet on join, \n`disable` -> disable greetings \n`screening` -> greet when membership screening is passed")]
        public async Task Greeting(CommandContext ctx, string option)
        {
            GreetingOption parsedOption = option.ToLower() switch
            {
                "disable"   => GreetingOption.DoNotGreet,
                "role"      => GreetingOption.GreetOnRole,
                "join"      => GreetingOption.GreetOnJoin,
                "screening" => GreetingOption.GreetOnScreening,
                _           => (GreetingOption)(-1)
            };

            if ((int)parsedOption is -1)
            {
                await ctx.RespondAsync("That doesn't appear to be a valid option!");
                return;
            }

            EnsureCancellationTokenCancellation(ctx.User.Id);

            bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

            if (!res) return;

            await _mediator.Send(new UpdateGuildConfigRequest(ctx.Guild.Id) { GreetingOption = parsedOption });
        }

        [Command]
        [Aliases("greeting-channel", "welcomechannel", "welcome-channel", "gc", "wc")]
        public async Task GreetingChannel(CommandContext ctx, DiscordChannel channel)
        {
            GuildConfigEntity? conf = await _mediator.Send(new GetGuildConfigRequest(ctx.Guild.Id));

            if (string.IsNullOrEmpty(conf.GreetingText))
            {
                await ctx.RespondAsync("Set a welcome message first!");
                return;
            }

            EnsureCancellationTokenCancellation(ctx.User.Id);

            bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

            if (!res) return;

            await _mediator.Send(new UpdateGuildConfigRequest(ctx.Guild.Id) { GreetingChannelId = channel.Id });
        }

        [Command]
        [Aliases("greeting-role", "welcomerole", "gr", "welcome-role", "wr")]
        [Description("What role to check for before greeting members. Cannot be @everyone.")]
        public async Task GreetingRole(CommandContext ctx, DiscordRole role)
        {
            if (role == ctx.Guild.EveryoneRole)
            {
                await ctx.RespondAsync("No, you cannot use the everyone role for that.");
                return;
            }

            EnsureCancellationTokenCancellation(ctx.User.Id);

            bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

            if (!res) return;

            await _mediator.Send(new UpdateGuildConfigRequest(ctx.Guild.Id) { VerificationRoleId = role.Id });
        }

        [Command]
        [Aliases("greeting-message", "welcomemessage", "wm", "gm")]
        [Description("What should I greet members with? Substitutions: \n`{u}` - Username \n`{@u}` - User ping \n`{s}` - Server name")]
        public async Task GreetingMessage(CommandContext ctx, [RemainingText] string message)
        {
            if (message.Length > 2000)
            {
                await ctx.RespondAsync("Welcome message must be 2000 characters or less!");
                return;
            }

            if (string.IsNullOrEmpty(message))
            {
                await ctx.RespondAsync("You must provide a message!");
                return;
            }

            EnsureCancellationTokenCancellation(ctx.User.Id);

            bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

            if (!res) return;

            await _mediator.Send(new UpdateGuildConfigRequest(ctx.Guild.Id) { GreetingText = message });
        }

        /// <summary>
        ///     Waits indefinitely for user confirmation unless the associated token is cancelled.
        /// </summary>
        /// <param name="user">The id of the user to assign a token to and wait for input from.</param>
        /// <param name="channel">The channel to send a message to, to request user input.</param>
        /// <returns>True if the user selected true, or false if the user selected no OR the cancellation token was cancelled.</returns>
        internal static async Task<bool> GetButtonConfirmationUserInputAsync(DiscordUser user, DiscordChannel channel)
        {
            DiscordMessageBuilder? builder = new DiscordMessageBuilder().WithContent("Are you sure?").AddComponents(_yesButton, _noButton);

            DiscordMessage?   message = await builder.SendAsync(channel);
            CancellationToken token   = GetTokenFromWaitQueue(user.Id);

            InteractivityResult<ComponentInteractionCreateEventArgs> interactivityResult = await channel.GetClient().GetInteractivity().WaitForButtonAsync(message, user, token);

            if (interactivityResult.TimedOut) // CT was yeeted. //
            {
                await message.ModifyAsync(b => b.WithContent("Cancelled!").AddComponents(_yesButtonDisabled, _noButtonDisabled));
                return false;
            }

            // Nobody likes 'This interaction failed'. //
            if (interactivityResult.Result.Id == _yesButton.CustomId)
            {
                await interactivityResult.Result
                                         .Interaction
                                         .CreateResponseAsync(InteractionResponseType.UpdateMessage, _confirmBuilder);

                return true;
            }
            await interactivityResult.Result
                                     .Interaction
                                     .CreateResponseAsync(InteractionResponseType.UpdateMessage, _declineBuilder);

            return false;
        }

        /// <summary>
        ///     Cancels and removes the token with the specified id if it exists.
        /// </summary>
        /// <param name="id">The id of the user to look up.</param>
        private static void EnsureCancellationTokenCancellation(ulong id)
        {
            if (_tokens.TryRemove(id, out CancellationTokenSource? token))
            {
                token.Cancel();
                token.Dispose();
            }
        }

        /// <summary>
        ///     Gets a <see cref="CancellationToken" />, creating one if necessary.
        /// </summary>
        /// <param name="id">The id of the user to assign the token to.</param>
        /// <returns>The returned or generated token.</returns>
        private static CancellationToken GetTokenFromWaitQueue(ulong id)
        {
            return _tokens.GetOrAdd(id, id => _tokens[id] = new()).Token;
        }


        [Group("phishing")]
        [Aliases("phish", "psh")]
        
        [Description("Phishing-related settings.")]
        public sealed class EditPhishingModule : BaseCommandModule
        {
            private readonly IMediator _mediator;
            public EditPhishingModule(IMediator mediator) => _mediator = mediator;

            [Command]
            [Description("Enables scanning for phishing links.")]
            public async Task Enable(CommandContext ctx)
            {
                EnsureCancellationTokenCancellation(ctx.User.Id);

                bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                if (!res) return;

                await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { DetectPhishingLinks = true });
            }

            [Command]
            [Description("Disables scanning for phishing links.")]
            public async Task Disable(CommandContext ctx)
            {
                EnsureCancellationTokenCancellation(ctx.User.Id);

                bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                if (!res) return;

                await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { DetectPhishingLinks = false });
            }

            [Command]
            [Aliases("delete_links", "delete-links", "dl")]
            [Description("Whether messages will be deleted when a phishing link is detected.")]
            public async Task DeleteLinks(CommandContext ctx, bool delete)
            {
                EnsureCancellationTokenCancellation(ctx.User.Id);

                bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                if (!res) return;

                await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { DeletePhishingLinks = delete });
            }

            [Command]
            [Description("The action to take when a phishing link is detected.\nOptions: Kick, Ban, Note, None.\nNone will still log links, but they will not be attached to the user.")]
            public async Task Action(CommandContext ctx, string action)
            {
                var type = InfractionType.Pardon;
                if (!string.Equals("none", action, StringComparison.OrdinalIgnoreCase))
                {
                    if (!Enum.TryParse(action, true, out type))
                    {
                        await ctx.RespondAsync("I can't tell what you're trying to set.");
                        return;
                    }

                    if (type is not (InfractionType.Ban or InfractionType.Kick or InfractionType.Note))
                    {
                        await ctx.RespondAsync("Action must be of type Kick, Ban, Note, or None.");
                    }
                }

                EnsureCancellationTokenCancellation(ctx.User.Id);

                bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                if (!res) return;

                GuildModConfigEntity? config = await _mediator.Send(new GetGuildModConfigRequest(ctx.Guild.Id));

                config.NamedInfractionSteps.Remove(AutoModConstants.PhishingLinkDetected);


                if (type is not InfractionType.Pardon)
                    config.NamedInfractionSteps[AutoModConstants.PhishingLinkDetected] = new() { Type = type };


                await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { NamedInfractionSteps = config.NamedInfractionSteps });
            }
        }


        [Group("invite")]
        [Aliases("invites", "inv")]
        
        [Description("Invite related settings.")]
        public sealed class EditInviteModule : BaseCommandModule
        {
            private readonly IMediator _mediator;
            public EditInviteModule(IMediator mediator) => _mediator = mediator;

            [Command]
            [Aliases("so", "scan")]
            [Description("Whether or not an effort should be made to check the origin of an invite before taking action. \nLow impact to AutoMod latency.")]
            public async Task ScanOrigin(CommandContext ctx, bool scan)
            {
                EnsureCancellationTokenCancellation(ctx.User.Id);

                bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                if (!res) return;

                await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { ScanInvites = scan });
            }

            [Command]
            [Aliases("warn", "wom")]
            [Description("Whether members should be warned for sending non-whitelisted invites. \nIf `auto-escalate-infractions` is set to true, the configured auto-mod setting will be used, else it will fallback to the configured infraction step depending on the user's current infraction count.")]
            public async Task WarnOnMatch(CommandContext ctx, bool warn)
            {
                EnsureCancellationTokenCancellation(ctx.User.Id);

                bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                if (!res) return;

                await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { WarnOnMatchedInvite = warn });
            }

            [Command]
            [Aliases("dom", "delete")]
            [Description("Whether or not invites will be deleted when they're detected in messages.")]
            public async Task DeleteOnMatch(CommandContext ctx, bool delete)
            {
                EnsureCancellationTokenCancellation(ctx.User.Id);

                bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                if (!res) return;

                await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { DeleteOnMatchedInvite = delete });
            }

            [Command]
            [Aliases("ma")]
            [Description("Whether or not to use the aggressive invite matching regex. \n`disc((ord)?(((app)?\\.com\\/invite)|(\\.gg)))\\/([A-z0-9-_]{2,})`")]
            public async Task MatchAggressively(CommandContext ctx, bool match)
            {
                EnsureCancellationTokenCancellation(ctx.User.Id);

                bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                if (!res) return;

                await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { UseAggressiveRegex = match });
            }

            [Group("whitelist")]
            
            [Description("Invite whitelist related settings.")]
            public sealed class EditInviteWhitelistModule : BaseCommandModule
            {
                private readonly IMediator _mediator;
                public EditInviteWhitelistModule(IMediator mediator) => _mediator = mediator;

                [Command]
                public async Task Add(CommandContext ctx, string invite)
                {
                    DiscordInvite inviteObj;
                    try
                    {
                        inviteObj = await ctx.Client.GetInviteByCodeAsync(invite.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Last());
                    }
                    catch
                    {
                        await ctx.RespondAsync("That doesn't appear to be a valid invite, sorry!");
                        return;
                    }

                    if (inviteObj.Guild.Id == ctx.Guild.Id)
                    {
                        await ctx.RespondAsync("Don't worry, invites from your server are automatically whitelisted!");
                        return;
                    }

                    if (inviteObj.IsRevoked || inviteObj.MaxAge < 0)
                    {
                        await ctx.RespondAsync("That invite is expired!");
                        return;
                    }

                    EnsureCancellationTokenCancellation(ctx.User.Id);

                    if (inviteObj.Guild.VanityUrlCode is null || inviteObj.Guild.VanityUrlCode != inviteObj.Code)
                        await ctx.RespondAsync(":warning: Warning, this code is not a vanity code!");

                    bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                    if (!res) return;

                    GuildModConfigEntity? config = await _mediator.Send(new GetGuildModConfigRequest(ctx.Guild.Id));
                    config.AllowedInvites.Add(new() { GuildId = ctx.Guild.Id, InviteGuildId = inviteObj.Guild.Id, VanityURL = inviteObj.Guild.VanityUrlCode ?? inviteObj.Code });

                    await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { AllowedInvites = config.AllowedInvites });
                }

                [Command]
                public async Task Add(CommandContext ctx, [RemainingText] params string[] invites)
                {
                    EnsureCancellationTokenCancellation(ctx.User.Id);

                    bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                    if (!res) return;

                    GuildModConfigEntity? config = await _mediator.Send(new GetGuildModConfigRequest(ctx.Guild.Id));

                    foreach (string inviteCode in invites)
                    {
                        DiscordInvite inviteObj;
                        try
                        {
                            inviteObj = await ctx.Client.GetInviteByCodeAsync(inviteCode.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Last());
                        }
                        catch { continue; }

                        if (inviteObj.Guild.Id == ctx.Guild.Id)
                            continue;

                        config.AllowedInvites.Add(new() { GuildId = ctx.Guild.Id, InviteGuildId = inviteObj.Guild.Id, VanityURL = inviteObj.Guild.VanityUrlCode ?? inviteObj.Code });
                    }

                    await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { AllowedInvites = config.AllowedInvites });
                }

                [Command]
                public async Task Remove(CommandContext ctx, string invite)
                {
                    DiscordInvite inviteObj;
                    try
                    {
                        inviteObj = await ctx.Client.GetInviteByCodeAsync(invite.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Last());
                    }
                    catch
                    {
                        await ctx.RespondAsync("That doesn't appear to be a valid invite, sorry!");
                        return;
                    }

                    if (inviteObj.Guild.Id == ctx.Guild.Id)
                    {
                        await ctx.RespondAsync("Don't worry, invites from your server are automatically whitelisted!");
                        return;
                    }

                    EnsureCancellationTokenCancellation(ctx.User.Id);

                    bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                    if (!res) return;

                    GuildModConfigEntity? config = await _mediator.Send(new GetGuildModConfigRequest(ctx.Guild.Id));

                    InviteEntity? inv = config.AllowedInvites.SingleOrDefault(i => i.VanityURL == inviteObj.Code);

                    if (inv is null) return;

                    config.AllowedInvites.Remove(new() { GuildId = ctx.Guild.Id, VanityURL = inviteObj.Guild.VanityUrlCode ?? inviteObj.Code });

                    await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { AllowedInvites = config.AllowedInvites });
                }

                [Command]
                [RequireFlag(UserFlag.EscalatedStaff)]
                public async Task Clear(CommandContext ctx)
                {
                    EnsureCancellationTokenCancellation(ctx.User.Id);

                    bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                    if (!res) return;

                    await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { AllowedInvites = Array.Empty<InviteEntity>().ToList() });
                }

                [Command]
                public async Task Enable(CommandContext ctx)
                {
                    EnsureCancellationTokenCancellation(ctx.User.Id);

                    bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                    if (!res) return;

                    await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { BlacklistInvites = true });
                }

                [Command]
                public async Task Disable(CommandContext ctx)
                {
                    EnsureCancellationTokenCancellation(ctx.User.Id);

                    bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                    if (!res) return;

                    await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { BlacklistInvites = false });
                }
            }
        }

        [Group("log")]
        
        [Description("Logging related settings.")]
        public sealed class EditLogModule : BaseCommandModule
        {
            private readonly IMediator _mediator;
            public EditLogModule(IMediator mediator) => _mediator = mediator;

            [Command]
            [Description("Edit the channel I logs infractions, users, etc to!")]
            public async Task Channel(CommandContext ctx, DiscordChannel channel)
            {
                if (!channel.PermissionsFor(ctx.Guild.CurrentMember).HasPermission(FlagConstants.LoggingPermissions))
                {
                    await ctx.RespondAsync($"I don't have proper permissions to log there! I need {FlagConstants.LoggingPermissions.ToPermissionString()}");
                    return;
                }

                EnsureCancellationTokenCancellation(ctx.User.Id);

                bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                if (!res) return;

                await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { LoggingChannel = channel.Id });
            }

            [Command("member-joins")]
            [Aliases("members-joining", "mj")]
            [Description("Edit whether or not I log members that join")]
            public async Task MembersJoin(CommandContext ctx, bool log)
            {
                EnsureCancellationTokenCancellation(ctx.User.Id);

                bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                if (!res) return;

                await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { LogMembersJoining = log });
            }

            [Command("member-leaves")]
            [Aliases("members-leaving", "ml")]
            [Description("Edit whether or not I log members that leave")]
            public async Task MembersLeave(CommandContext ctx, bool log)
            {
                EnsureCancellationTokenCancellation(ctx.User.Id);

                bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                if (!res) return;

                await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { LogMembersLeaving = log });
            }

            [Command("message-edits")]
            [Description("Whether or not I log message edits and deletions. Requires a log channel to be set.")]
            public async Task MessageEdits(CommandContext ctx, bool log)
            {
                EnsureCancellationTokenCancellation(ctx.User.Id);

                bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                if (!res) return;

                await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { LogMessageChanges = log });
            }
        }

        [Group("infractions")]
        
        [Aliases("infraction", "inf")]
        [Description("Infraction related settings.")]
        public sealed class EditInfractionModule : BaseCommandModule
        {
            private readonly IMediator _mediator;
            public EditInfractionModule(IMediator mediator) => _mediator = mediator;

            [Command]
            [Aliases("escalate", "esc")]
            [Description("Whether strikes should be automatically escalated. "                                                                                     +
                         "\n\n In the case of auto-mod, if a category does not have a defined action, strikes are used instead.\n"                                 +
                         "If this is set to true, AutoMod will attempt to use the configured action depending on how many infractions the user currently has.\n\n" +
                         "For manual strikes, if this is enabled, when a user has >= 5 strikes, moderators will be prompted if they want to escalate, which will follow the same procedure.")]
            public async Task AutoEscalate(CommandContext ctx, bool escalate)
            {
                EnsureCancellationTokenCancellation(ctx.User.Id);

                bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                if (!res) return;

                await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { EscalateInfractions = escalate });
            }

            [Command]
            [Description("Adds or overwrites an action for auto-mod. \nTo see available options, use `config view auto-mod-options`" +
                         "Available punishments: Ignore, Kick, Ban, SoftBan, Mute, Strike\n\n"                                       +
                         "**A note about AutoMod**: If `Ignore` is chosen, AutoMod will add a note to the user. Notes do not notify the user.")]
            public async Task Add(CommandContext ctx, string option, InfractionType type, TimeSpan? duration = null)
            {
                if (!AutoModConstants.ActionStrings.ContainsKey(option))
                {
                    await ctx.RespondAsync("Sorry, but that doesn't seem to be a valid option.");
                    return;
                }

                EnsureCancellationTokenCancellation(ctx.User.Id);

                bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                if (!res) return;

                GuildModConfigEntity? config = await _mediator.Send(new GetGuildModConfigRequest(ctx.Guild.Id));

                if (config.NamedInfractionSteps.TryGetValue(option, out InfractionStepEntity? action))
                {
                    action.Type     = type;
                    action.Duration = NpgsqlTimeSpan.ToNpgsqlTimeSpan(duration ?? TimeSpan.Zero);
                }
                else
                {
                    config.NamedInfractionSteps[option] = new()
                    {
                        Type     = type,
                        Config   = config,
                        Duration = NpgsqlTimeSpan.ToNpgsqlTimeSpan(duration ?? TimeSpan.Zero),
                    };
                }

                await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { NamedInfractionSteps = config.NamedInfractionSteps });
            }


            [Command]
            [Description("Removes a defined AutoMod action. See `config view auto-mod-actions` for a full list.")]
            public async Task Remove(CommandContext ctx, string option, InfractionType type, TimeSpan? duration = null)
            {
                if (!AutoModConstants.ActionStrings.ContainsKey(option))
                {
                    await ctx.RespondAsync("Sorry, but that doesn't seem to be a valid option.");
                    return;
                }

                EnsureCancellationTokenCancellation(ctx.User.Id);

                bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                if (!res) return;

                GuildModConfigEntity? config = await _mediator.Send(new GetGuildModConfigRequest(ctx.Guild.Id));

                if (config.NamedInfractionSteps.Remove(option))
                    await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { NamedInfractionSteps = config.NamedInfractionSteps });
            }

            [Group("steps")]
            [Description("Infraction step related settings.")]
            public sealed class InfractionStepsModule : BaseCommandModule
            {
                private readonly IMediator _mediator;
                public InfractionStepsModule(IMediator mediator) => _mediator = mediator;

                [Command]
                [Description("Adds a new infraction step. This action will be used when the user has **`n`** infractions.\n\n"         +
                             "If the infraction step count (see `config view`) is 2, when a user has one strike\n"                     +
                             "(or strike that were escalated), and the second infraction step is set to a 10 minute mute,"             +
                             "they will be muted for 10 minutes the next time they are struck.\n\n"                                    +
                             "Duration is only applicable to Mute and SoftBan.\n\n"                                                    +
                             "Available option types: Strike, Kick, Mute, SoftBan, Ban, Ignore. \nThese are case **in**sensitive.\n\n" +
                             "A note on `Ignore`: If the step is set to ignore, AutoMod will add a note to the user. The strike command will escalate to ban if the current step is ignore.")]
                public async Task Add(CommandContext ctx, InfractionType type, [RemainingText] TimeSpan? duration = null)
                {
                    EnsureCancellationTokenCancellation(ctx.User.Id);

                    bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                    if (!res) return;

                    GuildModConfigEntity? conf = await _mediator.Send(new GetGuildModConfigRequest(ctx.Guild.Id));
                    conf.InfractionSteps.Add(new() { Type = type, Duration = duration.HasValue ? NpgsqlTimeSpan.ToNpgsqlTimeSpan(duration.Value) : NpgsqlTimeSpan.Zero });
                    await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { InfractionSteps = conf.InfractionSteps });
                }

                [Command]
                [Description("Edits an infraction step. `index` is the number of infractions. If you want to edit the third step (3 infractions), simply pass 3. \nAvailable option types: Strike, Kick, Mute, SoftBan, Ban, Ignore. \nThese are case **in**sensitive.\n\n")]
                public async Task Edit(CommandContext ctx, uint index, InfractionType type, TimeSpan? duration = null)
                {
                    GuildModConfigEntity? conf = await _mediator.Send(new GetGuildModConfigRequest(ctx.Guild.Id));
                    if (!conf.InfractionSteps.Any())
                    {
                        await ctx.RespondAsync("There are no infraction steps to edit.");
                        return;
                    }

                    if (index is 0 || index > conf.InfractionSteps.Count)
                    {
                        await ctx.RespondAsync($"Please choose an infraction between 1 and {conf.InfractionSteps.Count}");
                        return;
                    }

                    EnsureCancellationTokenCancellation(ctx.User.Id);

                    bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                    if (!res) return;
                    InfractionStepEntity? step = conf.InfractionSteps[(int)index - 1];

                    step.Duration = NpgsqlTimeSpan.ToNpgsqlTimeSpan(duration ?? TimeSpan.Zero);
                    step.Type     = type;

                    await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { InfractionSteps = conf.InfractionSteps });
                }

                [Command]
                [Description("Removes an infraction step at the given index. If you want to remove the third step (3 infractions) pass 3. \n**This cannot be undone!**\n" +
                             "All subsequent steps will be shifted left. If you want to edit a step, see `config edit infraction step edit`.")]
                public async Task Remove(CommandContext ctx, uint index)
                {
                    GuildModConfigEntity? conf = await _mediator.Send(new GetGuildModConfigRequest(ctx.Guild.Id));
                    if (!conf.InfractionSteps.Any())
                    {
                        await ctx.RespondAsync("There are no infraction steps to edit.");
                        return;
                    }

                    if (index is 0 || index > conf.InfractionSteps.Count)
                    {
                        await ctx.RespondAsync($"Please choose an infraction between 1 and {conf.InfractionSteps.Count}");
                        return;
                    }

                    EnsureCancellationTokenCancellation(ctx.User.Id);

                    bool res = await GetButtonConfirmationUserInputAsync(ctx.User, ctx.Channel);

                    if (!res) return;
                    conf.InfractionSteps.RemoveAt((int)index - 1);

                    await _mediator.Send(new UpdateGuildModConfigRequest(ctx.Guild.Id) { InfractionSteps = conf.InfractionSteps });
                }
            }
        }
    }
}*/