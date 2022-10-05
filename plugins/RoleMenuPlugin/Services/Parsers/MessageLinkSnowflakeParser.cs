using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Remora.Commands.Parsers;
using Remora.Commands.Results;
using Remora.Discord.Rest;
using Remora.Rest.Core;
using Remora.Results;
using Constants = Remora.Discord.API.Constants;

namespace RoleMenuPlugin.Parsers;

public class MessageLinkSnowflakeParser : AbstractTypeParser<Snowflake>
{
    private static readonly Regex _messageRegex = new(@"https://(?:\S+\.)?discord.com/channels/@me|\d{17,20}/(?<message>\d{17,20})");

    public override ValueTask<Result<Snowflake>> TryParseAsync(string token, CancellationToken ct = default)
        => _messageRegex.Match(token).Groups["message"] is { Length: > 0 } match
            ? Snowflake.TryParse(match.Value, out var result, Constants.DiscordEpoch)
                ? new(result.Value)
                : new(Result<Snowflake>.FromError(new ParsingError<Snowflake>(token, "The provided string does not resemble a valid message link.")))
            : new(Result<Snowflake>.FromError(new ParsingError<Snowflake>(token, "The provided string does not resemble a valid message link.")));

}