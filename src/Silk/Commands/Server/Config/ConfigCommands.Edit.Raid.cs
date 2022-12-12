using System.ComponentModel;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.MediatR.Guilds;
using Silk.Shared;
using Silk.Shared.Constants;
using Silk.Utilities;

namespace Silk.Commands.Server;

public partial class ConfigCommands
{
    public partial class EditConfigCommands
    {
        [Command("raid")]
        [RequireDiscordPermission(DiscordPermission.BanMembers)]
        public async Task<IResult> RaidAsync
        (
            [Option('t', "threshold")]
            [Description(
                        "The threshold before considering the situation to be a raid\n"            +
                        "For join velocity, this is calculated as join/s > threshold/decay\n"      +
                        "For cluster matching, this is calculated as count(cluster) > threshold\n" +
                        "For message raids, it's calculated in the same manner as clump matching"
                        )]
            int? threshold = null,

            [Option('d', "decay")]
            [Description(
                        "The decay time, in seconds, for raid detection. If no raid is detected beyond this threshold,\n" +
                        "the raid is considered complete, and no further action will be taken.\n"                         +
                        "This is a double-edged sword, and should be tinkered with based on typical server activity.\n\n" +
                        "Too high of a number would disallow genuine users from joining, and too low may cause false-negatives."
                        )]
            int? decay = null,

            [Option('e', "enabled")]
            [Description(
                        "Whether or not the raid detection is enabled.\n" +
                        "If disabled, the raid detection will not be performed, and the raid will not be detected."
                        )]
            bool? enabled = null
        )
        {
            if (threshold is < 3)
            {
                await _channels.CreateMessageAsync(_context.GetChannelID(), "A raid threshold of less than three is not recommended.");
                return Result<ReactionResult>.FromSuccess(new(Emojis.WarningId));
            }
            
            if (decay is < 10)
            {
                await _channels.CreateMessageAsync(_context.GetChannelID(), "A raid decay of less than ten seconds is not recommended.");
                return Result<ReactionResult>.FromSuccess(new(Emojis.WarningId));
            }

            var request = new UpdateGuildConfig.Request(_context.GuildID.Value)
            {
                DetectRaids   = enabled   ?? default(Optional<bool>),
                RaidCooldown  = decay     ?? default(Optional<int>),
                RaidThreshold = threshold ?? default(Optional<int>)
            };
            
            await _mediator.Send(request);
            
            return Result<ReactionResult>.FromSuccess(new(Emojis.ConfirmId));
        }
    }
}