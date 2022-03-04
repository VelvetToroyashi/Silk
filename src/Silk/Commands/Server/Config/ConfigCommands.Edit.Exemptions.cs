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
        private const string ExemptionDescription =
            "Exemptions allow targets to bypass automod (such as anti-spam)\n\n"                     +
            "\"Target\" refers to the target of the exemption, be it a user, role, or channel.\n"    +
            "Supports "                                                                              +
            $"`{nameof(ExemptionCoverage.AntiInvite)}`, `{nameof(ExemptionCoverage.AntiPhishing)}` " +
            $"`{nameof(ExemptionCoverage.EditLogging)}`, `{nameof(ExemptionCoverage.DeleteLogging)}`\n\n";
    
        [Command("exemptions", "exempt", "ex")]
        [Description(ExemptionDescription)]
        public async Task<IResult> EditExemptionsAsync
        (
            [Option("targets")] 
            [Description("Users, roles, and channels the exemption applies to.")]
            OneOf<IUser, IRole, IChannel>[] targets,
        
            [Option("add")]
            [Description("The exemption types to add to the targets.")]
            ExemptionCoverage[] add,
        
            [Option("remove")]
            [Description("The exemption types to remove from the targets.")] 
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
                if (exemptions.FirstOrDefault(x => x.TargetID == exemption.Item1) is { } existingExemption)
                {
                    existingExemption.Exemption |= flagsToAdd;
                    existingExemption.Exemption &= ~flagsToRemove;

                    if (existingExemption.Exemption is ExemptionCoverage.NonExemptMarker)
                        exemptions.Remove(existingExemption);
                }
                else
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