//TODO: This
/*using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using MediatR;
using Silk.Data.Entities;
using Silk.Data.MediatR.Infractions;
using Silk.Data.MediatR.Users;
using Silk.Utilities;
using Silk.Utilities.HelpFormatter;

namespace Silk.Commands.Moderation;

[HelpCategory(Categories.Mod)]
public class CasesCommand : BaseCommandModule
{
    private readonly IMediator _mediator;
    public CasesCommand(IMediator mediator) => _mediator = mediator;

    [Command]
    [RequireGuild]
    
    [Description("Check the cases of a user including notes.")]
    public async Task Cases(CommandContext ctx, DiscordUser user)
    {
        DiscordMessageBuilder? mBuilder = new DiscordMessageBuilder().WithReply(ctx.Message.Id);
        var                    eBuilder = new DiscordEmbedBuilder();

        InfractionEntity[]? infractions = (await _mediator.Send(new GetUserInfractionsRequest(ctx.Guild.Id, user.Id)))?.ToArray();
        bool                userExists  = await _mediator.Send(new GetUserRequest(ctx.Guild.Id, user.Id)) is not null;

        if (!userExists || infractions.Length is 0)
        {
            mBuilder.WithContent("User has no cases!");
            await ctx.RespondAsync(mBuilder);
        }
        else
        {
            var stringBuilder = new StringBuilder();
            foreach (InfractionEntity currentInfraction in infractions)
            {
                stringBuilder
                   .Append($"Case {currentInfraction.CaseNumber}: ")
                   .Append($"`{currentInfraction.Type.Humanize(LetterCasing.Title)}`\n");
                if (currentInfraction.Escalated)
                    stringBuilder.AppendLine("\n[ESCALATED FROM STRIKE] ");


                string? reason =
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
             #1#
        }
    }
}*/