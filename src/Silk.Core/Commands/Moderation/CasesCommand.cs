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
    public class CasesCommand : BaseCommandModule
    {
        private readonly IMediator _mediator;
        public CasesCommand(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Command]
        public async Task Cases(CommandContext ctx, DiscordUser user)
        {
            var mBuilder = new DiscordMessageBuilder().WithReply(ctx.Message.Id);;
            var eBuilder = new DiscordEmbedBuilder();

            User? userModel = await _mediator.Send(new UserRequest.Get { UserId = user.Id, GuildId = ctx.Guild.Id });
            
            if (userModel is null || !userModel.Infractions.Any())
            {
                mBuilder.WithContent("User has no cases!");
                await ctx.RespondAsync(mBuilder);
            }
            else
            {
                string cases = userModel.Infractions
                    .Select((i, n) => 
                        $"{n}: {i.InfractionType} by <@{i.Enforcer}>, " + 
                        $"Reason:\n{i.Reason[..100]}")
                    .Join("\n");
                eBuilder
                    .WithColor(DiscordColor.Gold)
                    .WithTitle($"Infractions for {user.Id}")
                    .WithDescription(cases);
                mBuilder.WithEmbed(eBuilder);
                
                await ctx.RespondAsync(mBuilder);
            }
        }
    }
}