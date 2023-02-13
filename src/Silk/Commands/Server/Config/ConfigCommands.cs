//TODO: This

using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mediator;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Data.MediatR.Guilds;
using Silk.Services.Data;
using Silk.Shared.Constants;
using Silk.Utilities;
using Silk.Utilities.HelpFormatter;
using IMessage = Remora.Discord.API.Abstractions.Objects.IMessage;

namespace Silk.Commands.Server;

[Group("config", "cfg")]
[Category(Categories.Server)]
[RequireContext(ChannelContext.Guild)]
[Description("Configure various settings for your server!")]
[RequireDiscordPermission(DiscordPermission.ManageMessages, DiscordPermission.KickMembers, Operator = LogicalOperator.And)]
public partial class ConfigCommands : CommandGroup
{
    private readonly ViewConfigCommands _viewConfig;

    public ConfigCommands(ViewConfigCommands viewConfig)
        => _viewConfig = viewConfig;

    [Command("view")]
    [Description("View the settings for your server.")]
    public Task<IResult> ViewConfigAsync() => _viewConfig.ViewAllAsync();
    
    [Group("view", "v")]
    [Description("View the settings for your server.")]
    public partial class ViewConfigCommands : CommandGroup
    {
        private readonly IMediator              _mediator;
        private readonly ITextCommandContext         _context;
        private readonly IDiscordRestGuildAPI   _guilds;
        private readonly IDiscordRestChannelAPI _channels;

        public ViewConfigCommands
        (
            IMediator              mediator,
            ITextCommandContext         context,
            IDiscordRestGuildAPI   guilds,
            IDiscordRestChannelAPI channels
        )
        {
            _mediator = mediator;
            _context  = context;
            _guilds   = guilds;
            _channels = channels;
        }
        
        //TODO: Add exemptions
        
        [Command("all", "a")]
        [Description("View all settings for your server. \n" +
                     "Each section can be configured with `config edit` and the respective section name.")]
        public async Task<IResult> ViewAllAsync()
        {
            var config    = await _mediator.Send(new GetGuildConfig.Request(_context.GuildID.Value));

            var guildResult = await _guilds.GetGuildAsync(_context.GuildID.Value);

            if (!guildResult.IsDefined(out var guild))
                return guildResult;

            var phishingAction = config.NamedInfractionSteps!.TryGetValue(AutoModConstants.PhishingLinkDetected, out var action) ? action.Type.ToString() : "Not configured";
            
            var contentBuilder = new StringBuilder();

            contentBuilder
               .Clear()
               .AppendLine($"{Emojis.SettingsEmoji} **General Config:**")
               .AppendLine()
               .AppendLine("**Greetings** | `greetings`")
               .AppendLine($"> Configured Greetings: {config.Greetings.Count}")
               .AppendLine()
               .AppendLine($"{Emojis.WrenchEmoji} **Moderation Config:**")
               .AppendLine()
               .AppendLine("**Logging** | `logging`")
               .AppendLine($"> {(config.Logging.UseWebhookLogging ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.WrenchEmoji} Use Webhook Logging")
               .AppendLine($"> {(config.Logging.UseMobileFriendlyLogging? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.NewEmoji} Use mobile-friendly Logging")
               .AppendLine($"> {(config.Logging.LogMemberJoins ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.JoinEmoji} Log members joining")
               .AppendLine($"> {(config.Logging.LogMemberLeaves ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.LeaveEmoji} Log members leaving")
               .AppendLine($"> {(config.Logging.LogMessageEdits ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.EditEmoji} Log message edits")
               .AppendLine($"> {(config.Logging.LogMessageDeletes ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.DeleteEmoji} Log message deletes")
               .AppendLine()
               .AppendLine("**Invites** | `invites`, `invite-whitelist`")
               .AppendLine($"> {(config.Invites.WhitelistEnabled ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} Whitelist enabled")
               .AppendLine($"> {(config.Invites.ScanOrigin ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.ScanEmoji} Scan invite origin")
               .AppendLine($"> {(config.Invites.WarnOnMatch ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.WarningEmoji} Warn on invite")
               .AppendLine($"> {(config.Invites.DeleteOnMatch ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.DeleteEmoji} Delete matched invite")
               .AppendLine($"> {(config.Invites.UseAggressiveRegex ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.NoteEmoji} Use aggressive invite matching")
               .AppendLine($"> Allowed invites: {(config.Invites.Whitelist.Count is 0 ? "None" : $"{config.Invites.Whitelist.Count} allowed invites [See `config view invites`]")}")
               .AppendLine()
               .AppendLine("**Infractions** | `infractions`")
               .AppendLine($"> Mute role: {(config.MuteRoleID.Value is 0 ? "Not set" : $"<@&{config.MuteRoleID}>")}")
               .AppendLine($"> {(config.UseNativeMute ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} Use native mutes (Requires Timeout Members permission)")
               .AppendLine($"> {(config.ProgressiveStriking ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} Escalate infractions")
               .AppendLine($"> Infraction steps: {(config.InfractionSteps.Count is var dictCount and not 0 ? $"{dictCount} steps [See `config view infractions`]" : "Not configured")}")
               .AppendLine($"> Infraction steps (named): {((config.NamedInfractionSteps?.Count ?? 0) is var infNameCount and not 0 ? $"{infNameCount} steps [See `config view infractions`]" : "Not configured")}")
               .AppendLine()
               .AppendLine("**Exemptions** | `exemptions`")
               .AppendLine($"> {(config.Exemptions.Any() ? $"Configured AutoMod Exemptions: {config.Exemptions.Count}" : "Not configured")}")
               .AppendLine()
               .AppendLine("**Anti-Phishing** | `phishing`")
               .AppendLine($"> {(config.DetectPhishingLinks ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.WarningEmoji} Detect Phishing Links")
               .AppendLine($"> {(config.BanSuspiciousUsernames ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.ScanEmoji} Ban Suspicious Usernames")
               .AppendLine($"> {(config.DeletePhishingLinks ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.DeleteEmoji} Delete Phishing Links")
               .AppendLine($"> {(action is not null ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} {Emojis.WrenchEmoji} Post-detection action: {phishingAction}")
               .AppendLine()
               .AppendLine($"Anti-Raid | `raid` {Emojis.NewEmoji} {Emojis.BetaEmoji}")
               .AppendLine($"> {(config.EnableRaidDetection ? Emojis.EnabledEmoji : Emojis.DisabledEmoji)} Enable raid-detection")
               .AppendLine($"> Raid threshold: {config.RaidDetectionThreshold} accounts")
               .AppendLine($"> Raid detection decay: {config.RaidCooldownSeconds} seconds");

            var embed = new Embed
            {
                Title       = $"Config for {guild.Name}!",
                Colour      = Color.Goldenrod,
                Description = contentBuilder.ToString()
            };

            return await _channels.CreateMessageAsync(_context.GetChannelID(), embeds: new[] { embed });
        }
    }
}