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
using Silk.Extensions;

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
                if (infractions.Length < 15)
                {
                    var sb = new StringBuilder();
                    for (var i = 0; i < infractions.Length; i++)
                    {
                        InfractionDTO currentInfraction = infractions[i];
                        sb.AppendLine($"Case {i + 1}: {currentInfraction.Type.Humanize(LetterCasing.Title)} by <@{currentInfraction.EnforcerId}>, " +
                                      $"Reason:\n{currentInfraction.Reason.Pull(..80)}");
                    }

                    eBuilder
                        .WithColor(DiscordColor.Gold)
                        .WithTitle($"Cases for {user.Id}")
                        .WithDescription(sb.ToString());
                    mBuilder.WithEmbed(eBuilder);

                    await ctx.RespondAsync(mBuilder);  
                }
                /*
                 * TODO: Pagination
                 */
            }
        }
    }
}