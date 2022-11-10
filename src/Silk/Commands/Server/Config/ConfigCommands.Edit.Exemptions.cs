using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using OneOf;
using Remora.Commands.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.DTOs.Guilds.Config;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Shared;
using Silk.Shared.Constants;

namespace Silk.Commands.Server;

public partial class ConfigCommands
{
    public partial class EditConfigCommands
    {
        private const string ExemptionDescription = """
            Exemptions allow targets to bypass Silk's automod (such as anti-spam).
            "Target" in this context refers to the target who is being exempted.
            A target can be a user, a role, or an entire channel.
            Supported exmeptions are (case-insensitive):
            `EditLogging`, `DeleteLogging`, `AntiPhishing` and `AntiInvite`
            """;    

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
            var config = await _mediator.Send(new GetGuildConfig.Request(_context.GuildID.Value));
            
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
                else if (flagsToAdd != default)
                {
                    var newExemption = new ExemptionEntity
                    {
                        Exemption  = flagsToAdd,
                        GuildID    = _context.GuildID.Value,
                        TargetID   = exemption.Item1,
                        TargetType = exemption.Item2,
                    };

                    exemptions.Add(newExemption);
                }
            }
            
            await _mediator.Send(new UpdateGuildConfig.Request(_context.GuildID.Value)
            {
                Exemptions = config.Exemptions
            });

            return Result<ReactionResult>.FromSuccess(new(Emojis.ConfirmId));

            (Snowflake, ExemptionTarget) GetSnowflake(OneOf<IUser, IRole, IChannel> oneOf)
                => oneOf.Match
                   (
                        static user => (user.ID, ExemptionTarget.User), 
                        static role => (role.ID, ExemptionTarget.Role), 
                        static channel => (channel.ID, ExemptionTarget.Channel)
                   );
        }
    }
}