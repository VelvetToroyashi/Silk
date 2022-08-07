using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Shared;
using Silk.Shared.Constants;

namespace Silk.Commands.Server;

public partial class ConfigCommands
{
    public partial class EditConfigCommands
    {
        [Command("phishing")]
        [SuppressMessage("ReSharper", "RedundantBlankLines", Justification = "Readability")]
        [Description("Edit the settings for phishing detection.")]
        public async Task<IResult> PhishingAsync
        (
            [Option("enabled")]
            [Description("Whether phishing detection should be enabled.")]
            bool?   enabled = null,
            
            [Option('u', "ban_suspicious_users")]
            [Description("Whether to ban users with suspicious usernames," +
                         " including but not limited to \"Discord Moderator\" and the likes."
                        )]
            bool? banSuspiciousUsers = null,
            
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

            var config = await _mediator.Send(new GetGuildConfig.Request(_context.GuildID.Value));
            
            if (action is not null)
                config!.NamedInfractionSteps[AutoModConstants.PhishingLinkDetected] = new() { Type = parsedAction.Value };

            await _mediator.Send(new UpdateGuildConfig.Request(_context.GuildID.Value)
            {
                DetectPhishingLinks  = enabled ?? default(Optional<bool>),
                DeletePhishingLinks  = !preserve,
                BanSuspiciousUsernames = banSuspiciousUsers ?? default(Optional<bool>),
                NamedInfractionSteps = config.NamedInfractionSteps

            });
            
            return Result<ReactionResult>.FromSuccess(new(Emojis.ConfirmId));
        }
    }
}