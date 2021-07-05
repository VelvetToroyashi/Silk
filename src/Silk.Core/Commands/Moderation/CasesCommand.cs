using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using MediatR;
using Silk.Core.Data.DTOs;
using Silk.Core.Data.MediatR.Infractions;
using Silk.Core.Data.MediatR.Users;
using Silk.Core.Data.Models;
using Silk.Core.Types;
using Silk.Core.Utilities;
using Silk.Core.Utilities.HelpFormatter;

namespace Silk.Core.Commands.Moderation
{
    [Experimental]
    [Category(Categories.Mod)] 
    public class CasesCommand : BaseCommandModule
    {
        private readonly IMediator _mediator;
        public CasesCommand(IMediator mediator) => _mediator = mediator;

        [Command]
        [RequireGuild]
        [RequireFlag(UserFlag.Staff)]
        [Description("Check the cases of a user including notes.")]
        public async Task Cases(CommandContext ctx, DiscordUser user)
        {
            DiscordMessageBuilder? mBuilder = new DiscordMessageBuilder().WithReply(ctx.Message.Id);
            var eBuilder = new DiscordEmbedBuilder();

            var infractions = (await _mediator.Send(new GetUserInfractionsRequest(ctx.Guild.Id, user.Id))).ToArray();
            bool userExists = await _mediator.Send(new GetUserRequest(ctx.Guild.Id, user.Id)) is not null;

            if (!userExists || infractions.Length is 0)
            {
                mBuilder.WithContent("User has no cases!");
                await ctx.RespondAsync(mBuilder);
            }
            else
            {
                var stringBuilder = new StringBuilder();
                foreach (InfractionDTO currentInfraction in infractions)
                {
                    stringBuilder
                        .Append($"Case {currentInfraction.CaseNumber}: ")
                        .Append($"`{currentInfraction.Type.Humanize(LetterCasing.Title)}`\n");
                    if (currentInfraction.EscalatedFromStrike)
                        stringBuilder.AppendLine("\n[ESCALATED FROM STRIKE] ");
                    

                    var reason = 
                        currentInfraction.Reason.Length <= 200 ?
                            $"Reason: {currentInfraction.Reason}" :
                            $"Reason: {currentInfraction.Reason[..200]}";
                    stringBuilder.AppendLine(reason);
                }

                eBuilder
                    .WithColor(DiscordColor.Gold)
                    .WithTitle($"Cases for {user.Id}")
                    .WithDescription(stringBuilder.ToString());
                mBuilder.WithEmbed(eBuilder);

                await ctx.RespondAsync(mBuilder);  
                
                /*
                 * TODO: Pagination
                 */
            }
        }
    }
}