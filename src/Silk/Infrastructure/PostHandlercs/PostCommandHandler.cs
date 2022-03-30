using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Extensions.Logging;
using Remora.Commands.Results;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Results;
using Remora.Discord.Commands.Services;
using Remora.Results;
using Sentry;
using Silk.Errors;
using Silk.Utilities.HelpFormatter;

namespace Silk;

public class PostCommandHandler : IPostExecutionEvent
{
    private readonly IHub _hub;
    private readonly MessageContext              _context;
    private readonly CommandHelpViewer           _help;
    private readonly ICommandPrefixMatcher       _prefix;
    private readonly IDiscordRestChannelAPI      _channels;
    private readonly ILogger<PostCommandHandler> _logger;

    public PostCommandHandler
    (
        IHub                        hub,
        MessageContext              context,
        CommandHelpViewer           help,
        ICommandPrefixMatcher       prefix,
        IDiscordRestChannelAPI      channels,
        ILogger<PostCommandHandler> logger
    )
    {
        _hub     = hub;
        _context  = context;
        _help     = help;
        _prefix   = prefix;
        _channels = channels;
        _logger   = logger;
    }

    public async Task<Result> AfterExecutionAsync(ICommandContext context, IResult commandResult, CancellationToken ct = default)
    {
        if (commandResult.IsSuccess)
            return Result.FromSuccess();

        var prefixResult = await _prefix.MatchesPrefixAsync(_context.Message.Content.Value, ct);
        
        if (!prefixResult.IsDefined(out var prefix) || !prefix.Matches)
            return Result.FromSuccess();

        var error = commandResult.Error;

        if (error is AggregateError ag)
            error = ag.Errors.First().Error;
        
        if (error is CommandNotFoundError)
            await _help.SendHelpAsync(_context.Message.Content.Value[prefix.ContentStartIndex..], _context.ChannelID);
        
        if (error is ConditionNotSatisfiedError)
            await HandleFailedConditionAsync(commandResult.Inner!.Inner!.Inner!.Inner.Error, ct);

        if (error is ExceptionError er)
            _hub.CaptureException(er.Exception);
        
        return Result.FromSuccess();
    }

    private async Task HandleFailedConditionAsync(IResultError conditionError, CancellationToken ct)
    {
        var message = conditionError.Message;

        var responseMessage = conditionError switch
        {
            SelfActionError sae       => sae.Message,
            PermissionDeniedError pne => $"As much as I'd love to, you're missing permissions to {pne.Permissions.Select(p => p.Humanize(LetterCasing.Title)).Humanize()}!",
            _                         => message
        };
        
        await _channels.CreateMessageAsync(_context.ChannelID, responseMessage, ct: ct);
    }
}