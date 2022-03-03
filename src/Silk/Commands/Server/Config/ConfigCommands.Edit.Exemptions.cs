using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using OneOf;
using Remora.Commands.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Data.MediatR.Guilds.Config;
using Silk.Shared.Constants;

namespace Silk.Commands.Server;

public partial class ConfigCommands
{
    public partial class EditConfigCommands
    {
        private const string ExemptionDescription = "Edits exemptions for your server. Exemptions allow targets to savely bypass specific automod actions (such as anti-spam)\n"                +
                                                    "Valid types are [case-insensitive]:\n"                                                                                                     +
                                                    $"{nameof(ExemptionCoverage.Invites)} - Exempts targets from being warned for sending invites (if configured)\n"                            +
                                                    $"{nameof(ExemptionCoverage.Spam)} - Exempts targets from being warned for spamming (if configured)\n"                                      +
                                                    $"{nameof(ExemptionCoverage.Phishing)} - Exempts targets from phishing actions. **It is strongly advised to only apply this to channels**." +
                                                    $"{nameof(ExemptionCoverage.MessageEdits)} - Exempts targets from having their messages message edits logged (if configured)\n"             +
                                                    $"{nameof(ExemptionCoverage.MessageDeletes)} - Exempts targets from having their messages message deletes logged (if configured)\n"         +
                                                    "\"Target\" refers to the target of the exemption, beit a user, role, or channel.";
    
        [Command("exemptions", "exempt", "ex")]
        [Description(ExemptionDescription)]
        public async Task<IResult> EditExemptionsAsync
        (
            [Option("targets")] 
            [Description("Targets to specified exemptions to. Specify roles, channels, or users.")]
            OneOf<IUser, IRole, IChannel>[] targets,
        
            [Option("add")]
            [Description("Targets to add from the specified exemptions from. Specify a role, channel, or user.\n" +
                         "Targets that have existing exemptions will be combined with the specified exemption types.")]
            ExemptionCoverage[] add,
        
            [Option("remove")]
            [Description($"The type(s) of exmeptions to remove.")] 
            ExemptionCoverage[] remove
        )
        {
            var config = await _mediator.Send(new GetGuildModConfig.Request(_context.GuildID.Value));
            
            var exemptions = config.Exemptions;

            var flagsToAdd    = !add   .Any() ? default : add   .Aggregate((current, exemption) => current | exemption);
            var flagsToRemove = !remove.Any() ? default : remove.Aggregate((current, exemption) => current | exemption);
            
            var flattened = targets.Select(GetSnowflake);
            
            foreach (var exemption in flattened)
            {
                if (exemptions.FirstOrDefault(x => x.TargetID == exemption.Item1) is not { } existingExemption)
                {
                    var newExemption = new ExemptionEntity()
                    {
                        Exemption  = flagsToAdd,
                        GuildID    = _context.GuildID.Value,
                        TargetID   = exemption.Item1,
                        TargetType = exemption.Item2,
                    };
                    
                    exemptions.Add(newExemption);
                }
                else
                {
                    existingExemption.Exemption |= flagsToAdd;
                    existingExemption.Exemption &= ~flagsToRemove;

                    if (existingExemption.Exemption is ExemptionCoverage.NonExemptMarker)
                        exemptions.Remove(existingExemption);
                }
            }
            
            await _mediator.Send(new UpdateGuildModConfig.Request(_context.GuildID.Value)
            {
                Exemptions = config.Exemptions
            });

            return await _channels.CreateReactionAsync(_context.ChannelID, _context.MessageID, $"_:{Emojis.ConfirmId}");

            (Snowflake, ExemptionTarget) GetSnowflake(OneOf<IUser, IRole, IChannel> oneOf)
                => ((Snowflake, ExemptionTarget))oneOf
                             .MapT0(u => (u.ID, ExemptionTarget.User))
                             .MapT1(r => (r.ID, ExemptionTarget.Role))
                             .MapT2(c => (c.ID, ExemptionTarget.Channel))
                             .Value;
        }
    }
}