using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Results;

namespace Silk;

/// <summary>
/// Handles sending error messages to a user when a slash command returns a non-success result.
/// </summary>
public class AfterSlashHandler : IPostExecutionEvent
{
    private readonly IDiscordRestInteractionAPI _interactions;
    public AfterSlashHandler(IDiscordRestInteractionAPI interactions)
    {
        _interactions = interactions;
    }

    public async Task<Result> AfterExecutionAsync(ICommandContext context, IResult commandResult, CancellationToken ct)
    {
        if (context is not IInteractionCommandContext ic || commandResult.IsSuccess)
            return Result.FromSuccess();
        
        /*
         * TODO: FIX THIS!!!
         *
         * It's not a great idea to return a message directly to the user, in case the response isn't human-friendly.
         *
         * A type-check should be performed to ensure that the message will be a human-friendly message, such as SlashError or the likes.
         *
         * No idea what this will entail, but it works for now.
         */
        await _interactions.CreateFollowupMessageAsync(ic.Interaction.ApplicationID, ic.Interaction.Token, commandResult.Error!.Message, ct: ct);
        
        return Result.FromSuccess();
    }
}