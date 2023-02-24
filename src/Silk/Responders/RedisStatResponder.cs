using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Gateway.Events;
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
        var unavailable = false;
        var present = gatewayEvent.Guild.IsT0 || gatewayEvent.Guild.AsT1.IsUnavailable.IsDefined(out unavailable);

        if (unavailable)
            return Result.FromSuccess();
        
        var db = _redis.GetDatabase();
        
        await db.StringIncrementAsync(ShardHelper.GetShardUserCountStatKey(_options.ShardIdentification!.ShardID), gatewayEvent.Guild.AsT0.MemberCount);
        
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

        var cacheKey = new KeyHelpers.GuildCacheKey(gatewayEvent.ID);
        
        if ((await _cache.TryGetValueAsync<IGuildCreate>(cacheKey, ct)).IsDefined(out var guild) || 
            (await _cache.TryGetPreviousValueAsync<IGuildCreate>(cacheKey)).IsDefined(out guild))
        {
            await db.StringDecrementAsync(ShardHelper.GetShardUserCountStatKey(_options.ShardIdentification!.ShardID), guild.Guild.AsT0.MemberCount);
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