using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Remora.Discord.Caching.Abstractions.Services;
using Remora.Discord.Caching.Services;
using Remora.Rest;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Extensions;
using Silk.Extensions.Remora;
using Silk.Shared.Constants;
using StackExchange.Redis;

namespace Silk.Services.Guild;


public class MemberScannerService
{
    private readonly CacheService                  _cache;
    private readonly ICacheProvider                _cacheProvider;
    private readonly IRestHttpClient               _rest;
    private readonly IConnectionMultiplexer        _redis;
    private readonly PhishingDetectionService      _phishing;
    private readonly ILogger<MemberScannerService> _logger;
    
    public MemberScannerService
    (
        CacheService cache,
        ICacheProvider cacheProvider,
        IRestHttpClient rest,
        IConnectionMultiplexer redis,
        PhishingDetectionService phishing,
        ILogger<MemberScannerService> logger
    )
    {
        _cache         = cache;
        _cacheProvider = cacheProvider;
        _rest          = rest;
        _redis         = redis;
        _phishing      = phishing;
        _logger        = logger;
    }
    
    public async Task<Result> ValidateCooldownAsync(Snowflake guildID)
    {
        var db = _redis.GetDatabase();

        var lastCheck = (string?)await db.StringGetAsync($"Silk:SuspiciousMemberCheck:{guildID}");
        var time      = lastCheck is null ? DateTimeOffset.UtcNow.AddHours(-7) : DateTimeOffset.Parse(lastCheck);

        var delta = DateTimeOffset.UtcNow - time;

        if (delta < TimeSpan.FromHours(6))
            return Result.FromError(new InvalidOperationError($"{Emojis.DeclineEmoji} Member scanning is only available every 6 hours. Check back {(DateTimeOffset.UtcNow + (TimeSpan.FromHours(6) - delta)).ToTimestamp()}!"));
        
        return Result.FromSuccess();
    }
    
    public async Task<Result<IReadOnlyList<Snowflake>>> GetSuspicousMembersAsync(Snowflake guildID, CancellationToken ct = default)
    {
        _logger.LogTrace("Begin member scan");

        var memberResult = await _rest.GetGuildMembersAsync(_cacheProvider, guildID);
        
        if (!memberResult.IsDefined(out var members))
            return Result<IReadOnlyList<Snowflake>>.FromError(new NotFoundError("There was an issue while fetching server members. Sorry."));
        
        _logger.LogTrace("Received {MemberCount} members, filtering...", members.Count);
        
        var query = members.Count > 5_000 ? members.AsParallel() : members.AsEnumerable();
        
        var phishing = query
                      .Select(g => g.User.Value)
                      .Where(u => u.IsBot.IsDefined(out var bot) && !bot)
                      .Where(u => _phishing.IsSuspectedPhishingUsername(u.Username).IsSuspicious)
                      .Select(u => u.ID)
                      .ToArray();
        
        _logger.LogDebug("Filtered {Members} as suspicious from initial set", phishing.Length);

        var db = _redis.GetDatabase();

        await db.StringSetAsync($"Silk:SuspiciousMemberCheck:{guildID}", DateTimeOffset.UtcNow.ToString());
        
        await _cache.CacheAsync<IReadOnlyList<Snowflake>>($"Silk:SuspiciousMemberCheck:{guildID}:Members", phishing, ct);
        
        _logger.LogDebug("Cached members list");
        
        return phishing;
    }
}