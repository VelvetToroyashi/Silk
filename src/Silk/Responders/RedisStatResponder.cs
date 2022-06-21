using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Caching;
using Remora.Discord.Caching.Services;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Utilities;
using StackExchange.Redis;

namespace Silk.Responders;

public class RedisStatResponder : IResponder<IReady>, IResponder<IGuildCreate>, IResponder<IGuildDelete>, IResponder<IGuildMemberAdd>, IResponder<IGuildMemberRemove>
{
    private readonly CacheService                _cache;
    private readonly IConnectionMultiplexer      _redis;
    private readonly DiscordGatewayClientOptions _options;
    
    public RedisStatResponder(CacheService cache, IConnectionMultiplexer redis, IOptions<DiscordGatewayClientOptions> options)
    {
        _cache = cache;
        _redis   = redis;
        _options = options.Value;
    }

    public async Task<Result> RespondAsync(IReady gatewayEvent, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();

        var key = ShardHelper.GetShardGuildCountStatKey(_options.ShardIdentification!.ShardID);
        await db.StringSetAsync(key, (long)gatewayEvent.Guilds.Count);

        key = ShardHelper.GetShardUserCountStatKey(_options.ShardIdentification!.ShardID);

        await db.StringSetAsync(key, 0);

        return Result.FromSuccess();
    }

    public async Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct = default)
    {
        var present = gatewayEvent.IsUnavailable.IsDefined(out var unavailable);

        if (unavailable)
            return Result.FromSuccess();
        
        var db = _redis.GetDatabase();
        
        if (gatewayEvent.MemberCount.IsDefined(out var gwm))
            await db.StringIncrementAsync(ShardHelper.GetShardUserCountStatKey(_options.ShardIdentification!.ShardID), gwm);

        if (present)
            return Result.FromSuccess();
        
        var key = ShardHelper.GetShardGuildCountStatKey(_options.ShardIdentification!.ShardID);
        await db.StringIncrementAsync(key);
        
        return Result.FromSuccess();
    }

    public async Task<Result> RespondAsync(IGuildDelete gatewayEvent, CancellationToken ct = default)
    {
        if (gatewayEvent.IsUnavailable.IsDefined(out var unavailable) && unavailable)
            return Result.FromSuccess();
        
        var db = _redis.GetDatabase();
        
        var key = ShardHelper.GetShardGuildCountStatKey(_options.ShardIdentification!.ShardID);
        await db.StringDecrementAsync(key);

        var cacheKey = KeyHelpers.CreateGuildCacheKey(gatewayEvent.ID);
        
        if ((await _cache.TryGetValueAsync<IGuildCreate>(cacheKey, ct)).IsDefined(out var guild) || 
            (await _cache.TryGetPreviousValueAsync<IGuildCreate>(cacheKey)).IsDefined(out guild))
        {
            if (!guild.MemberCount.IsDefined(out var gwm))
                return Result.FromSuccess();
            
            await db.StringDecrementAsync(ShardHelper.GetShardUserCountStatKey(_options.ShardIdentification!.ShardID), gwm);
        }
        
        return Result.FromSuccess();
    }

    public async Task<Result> RespondAsync(IGuildMemberAdd gatewayEvent, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        
        await db.StringIncrementAsync(ShardHelper.GetShardUserCountStatKey(_options.ShardIdentification!.ShardID));
        
        return Result.FromSuccess();
    }

    public async Task<Result> RespondAsync(IGuildMemberRemove gatewayEvent, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();

        await db.StringDecrementAsync(ShardHelper.GetShardUserCountStatKey(_options.ShardIdentification!.ShardID));
        
        return Result.FromSuccess();
    }
    
}