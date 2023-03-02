using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Remora.Commands.Parsers;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Rest.Core;
using Remora.Results;

namespace Silk;

public class MessageParser : AbstractTypeParser<IMessage>
{
    private readonly IOperationContext _context;
    private readonly IDiscordRestChannelAPI _channels;
    
    private readonly Regex _messageLinkRegex = new(@"^https?:\/\/(?:(?:canary|ptb)\.)?discord(?:app)?\.com\/channels\/\d+\/(?<CHANNEL>\d+)\/(?<MESSAGE>\d+)$");
    
    public MessageParser(IOperationContext context, IDiscordRestChannelAPI channels)
    {
        _context = context;
        _channels = channels;
    }

    public override async ValueTask<Result<IMessage>> TryParseAsync(string token, CancellationToken ct = default)
    {
        Snowflake? channel = null;
        
        if (!Snowflake.TryParse(token, out var message, Constants.DiscordEpoch))
        {
            var match = _messageLinkRegex.Match(token);
            if (match.Success)
            {
                Snowflake.TryParse(match.Groups["CHANNEL"].Value, out channel, Constants.DiscordEpoch);
                Snowflake.TryParse(match.Groups["MESSAGE"].Value, out message, Constants.DiscordEpoch);
            }
        }

        channel ??= _context switch
        {
            ITextCommandContext messageContext => messageContext.Message.ChannelID.IsDefined(out var chn) ? chn : null,
            IInteractionCommandContext interactionContext => interactionContext.Interaction.ChannelID.IsDefined(out var chn) ? chn : null,
            _ => null
        };
        
        if (!message.HasValue)
            return Result<IMessage>.FromError(new ArgumentInvalidError(nameof(token), "Could not parse a message ID from the given token."));
        
        return await _channels.GetChannelMessageAsync(channel.Value, message.Value, ct);
    }

    public override ValueTask<Result<IMessage>> TryParseAsync(IReadOnlyList<string> tokens, CancellationToken ct = default)
    {
        if (!tokens.Any())
            return ValueTask.FromResult(Result<IMessage>.FromError(new InvalidOperationError("No tokens were provided.")));

        return TryParseAsync(tokens.First(), ct);
    }
}