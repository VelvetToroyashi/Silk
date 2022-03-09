using System.Threading;
using System.Threading.Tasks;
using Remora.Commands.Results;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Results;
using Silk.Utilities.HelpFormatter;

namespace Silk;

public class PostCommandHandler : IPostExecutionEvent
{
    private readonly MessageContext         _context;
    private readonly CommandHelpViewer      _help;
    private readonly ICommandPrefixMatcher  _preifx;
    private readonly IDiscordRestChannelAPI _channels;
    
    public PostCommandHandler(MessageContext context, CommandHelpViewer help, ICommandPrefixMatcher preifx, IDiscordRestChannelAPI channels)
    {
        _context       = context;
        _help          = help;
        _preifx        = preifx;
        _channels = channels;
    }

    public async Task<Result> AfterExecutionAsync(ICommandContext context, IResult commandResult, CancellationToken ct = default)
    {
        if (commandResult.IsSuccess)
            return Result.FromSuccess();

        var prefixResult = await _preifx.MatchesPrefixAsync(_context.Message.Content.Value, ct);
        
        if (!prefixResult.IsDefined(out var prefix) || !prefix.Matches)
            return Result.FromSuccess();

        if (commandResult.Error is CommandNotFoundError)
            await _help.SendHelpAsync(_context.Message.Content.Value[prefix.ContentStartIndex..], _context.ChannelID);
        
        if (commandResult.Error is ConditionNotSatisfiedError)
            await HandleFailedConditionAsync(commandResult.Inner!.Inner!.Inner!.Inner!.Error!, ct);

        return Result.FromSuccess();
    }

    private async Task HandleFailedConditionAsync(IResultError conditionError, CancellationToken ct)
    {
        await _channels.CreateMessageAsync(_context.ChannelID, conditionError.Message, ct: ct);
    }
}