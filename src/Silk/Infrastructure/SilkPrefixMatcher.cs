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
    
    private readonly MessageContext      _context;
    private readonly IDiscordRestUserAPI _users;
    private readonly IPrefixCacheService _prefixCache;
    
    public SilkPrefixMatcher(MessageContext context, IDiscordRestUserAPI users, IPrefixCacheService prefixCache)
    {
        _context          = context;
        _users            = users;
        _prefixCache = prefixCache;
    }

    public async ValueTask<Result<(bool Matches, int ContentStartIndex)>>
        MatchesPrefixAsync(string content, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(content))
            return Result<(bool Matches, int ContentStartIndex)>.FromSuccess((false, 0));
        
        if (!_context.GuildID.IsDefined(out var guildID))
            return Result<(bool Matches, int ContentStartIndex)>.FromSuccess((true, 0));

        var prefix = _prefixCache.RetrievePrefix(guildID);
        
        if (content.StartsWith(prefix))
            return Result<(bool Matches, int ContentStartIndex)>.FromSuccess((true, prefix.Length));
        
        var selfResult = await _users.GetCurrentUserAsync(ct);
        
        if (!selfResult.IsSuccess)
            return Result<(bool Matches, int ContentStartIndex)>.FromError(selfResult.Error);
        
        var match = MentionRegex.Match(content);
        
        if (match.Success && match.Groups["ID"].Value == selfResult.Entity.ID.ToString())
            return Result<(bool Matches, int ContentStartIndex)>.FromSuccess((true, match.Length));
        
        return Result<(bool Matches, int ContentStartIndex)>.FromSuccess((false, 0));
    }
}