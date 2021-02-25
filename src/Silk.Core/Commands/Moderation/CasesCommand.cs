using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;
using Silk.Core.Utilities;
using Silk.Data.MediatR;
using Silk.Data.Models;
using Silk.Extensions;

namespace Silk.Core.Commands.Moderation
{
    [Experimental]
    // Read the VC chat; give me terrible ideas to implement and @ me
    public class CasesCommand : BaseCommandModule
    {
        private readonly IMediator _mediator;
        public CasesCommand(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Command]
        [RequireFlag(UserFlag.Staff)]
        public async Task Cases(CommandContext ctx, DiscordUser user)
        {
            var mBuilder = new DiscordMessageBuilder().WithReply(ctx.Message.Id);
            var eBuilder = new DiscordEmbedBuilder();

            User? userModel = await _mediator.Send(new UserRequest.Get(ctx.Guild.Id, user.Id));
            
            if (userModel is null || !userModel.Infractions.Any())
            {
                mBuilder.WithContent("User has no cases!");
                await ctx.RespondAsync(mBuilder);
            }
            else
            {
                string cases = userModel.Infractions
                    .OrderBy(i => i.InfractionTime)
                    .Select((i, n) =>
                    {
                        var s = $"{n + 1}: {i.InfractionType} by <@{i.Enforcer}>, ";
                        s += $"Reason:\n{i.Reason[..(i.Reason.Length > 100 ? 100 : ^0)]}";
                        return s;
                    })
                    .Join("\n");
                eBuilder
                    .WithColor(DiscordColor.Gold)
                    .WithTitle($"Cases for {user.Id}")
                    .WithDescription(cases);
                mBuilder.WithEmbed(eBuilder);
                
                await ctx.RespondAsync(mBuilder);
            }
        }
    }
}