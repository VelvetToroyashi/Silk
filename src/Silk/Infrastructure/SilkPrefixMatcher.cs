using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Results;
using Silk.Services.Interfaces;

namespace Silk;

public class SilkPrefixMatcher : ICommandPrefixMatcher
{
    private static readonly Regex MentionRegex = new(@"^(<@!?(?<ID>\d+)> ?)");
    
    private readonly IMessageContext _context;
    private readonly IDiscordRestUserAPI _users;
    private readonly IPrefixCacheService _prefixCache;
    
    public SilkPrefixMatcher(IMessageContext context, IDiscordRestUserAPI users, IPrefixCacheService prefixCache)
    {
        _context     = context;
        _users       = users;
        _prefixCache = prefixCache;
    }

    public async ValueTask<Result<(bool Matches, int ContentStartIndex)>>
        MatchesPrefixAsync(string content, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(content))
            return Result<(bool, int)>.FromSuccess((false, 0));

        if (!_context.GuildID.IsDefined(out var guildID))
            return Result<(bool, int)>.FromSuccess((true, 0));

        var selfResult = await _users.GetCurrentUserAsync(ct);
        
        if (!selfResult.IsSuccess)
            return Result<(bool, int)>.FromError(selfResult.Error);

        var match = MentionRegex.Match(content);

        if (match.Success && match.Groups["ID"].Value == selfResult.Entity.ID.ToString())
            return Result<(bool, int)>.FromSuccess((true, match.Length));

        var prefix = await _prefixCache.RetrievePrefixAsync(guildID);

        if (!content.StartsWith(prefix) || content.Length == prefix.Length)
            return Result<(bool, int)>.FromSuccess((false, 0));

        var contentStartIndex = prefix.Length;
        var charAfterPrefix   = content[prefix.Length];

        if (charAfterPrefix == ' ')
            contentStartIndex = GetContentStartIndexFromSpace(content, prefix.Length);

        return Result<(bool, int)>.FromSuccess((true, contentStartIndex));
    }

    private static int GetContentStartIndexFromSpace(string content, int startIndex)
    {
        for (int i = startIndex; i < content.Length; ++i)
            if (content[i] != ' ')
                return i;

        return startIndex;
    }
}