using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Caching;
using Remora.Discord.Caching.Services;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Utilities;
using StackExchange.Redis;

namespace Silk.Responders;

/// <summary>
///     Caches the current user data based on the <see cref="IReady" /> event data.
/// </summary>
public class ReadyUserCacher : IResponder<IReady>
{
    private readonly CacheService           _cache;
    private readonly IShardIdentification   _shard;
    private readonly IConnectionMultiplexer _redis;
    
    public ReadyUserCacher
    (
        CacheService cache,
        IShardIdentification shard,
        IConnectionMultiplexer redis
    )
    {
        _cache = cache;
        _shard = shard;
        _redis = redis;
    }

    public async Task<Result> RespondAsync(IReady gatewayEvent, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        await db.StringSetAsync(ShardHelper.GetShardUserCountStatKey(_shard.ShardID), 0);
        
        await _cache.CacheAsync(new KeyHelpers.CurrentUserCacheKey(), gatewayEvent.User, ct);
        
        return Result.FromSuccess();
    }
}