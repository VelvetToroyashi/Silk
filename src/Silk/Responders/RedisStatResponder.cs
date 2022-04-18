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

public class RedisStatResponder : IResponder<IReady>, IResponder<IGuildCreate>, IResponder<IGuildDelete>, IResponder<IGuildMemberAdd>, IResponder<IGuildMemberRemove>, IResponder<IGuildMembersChunk>
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

        return Result.FromSuccess();
    }

    public async Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct = default)
    {
        var present = gatewayEvent.IsUnavailable.IsDefined(out var unavailable);

        if (unavailable)
            return Result.FromSuccess();
        
        var db = _redis.GetDatabase();
        
        if (gatewayEvent.MemberCount.IsDefined(out var gwm) || gatewayEvent.ApproximateMemberCount.IsDefined(out gwm))
        {
            var current = await db.StringGetAsync(ShardHelper.GetShardUserCountStatKey(_options.ShardIdentification!.ShardID));
            
            await db.StringSetAsync(ShardHelper.GetShardUserCountStatKey(_options.ShardIdentification!.ShardID), (long)current + gwm);
        }
        
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
        
        if ((await _cache.TryGetValueAsync<IGuild>(cacheKey, ct)).IsDefined(out var guild) || 
            (await _cache.TryGetPreviousValueAsync<IGuild>(cacheKey)).IsDefined(out guild))
        {
            if (!guild.MemberCount.IsDefined(out var gwm))
                return Result.FromSuccess();
            
            var current = await db.StringGetAsync(ShardHelper.GetShardUserCountStatKey(_options.ShardIdentification!.ShardID));
            
            await db.StringSetAsync(ShardHelper.GetShardUserCountStatKey(_options.ShardIdentification!.ShardID), (long)current - gwm);
        }
        
        return Result.FromSuccess();
    }

    public async Task<Result> RespondAsync(IGuildMemberAdd gatewayEvent, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        
        var key = ShardHelper.GetShardUserCountStatKey(_options.ShardIdentification!.ShardID);
        await db.StringIncrementAsync(key);
        
        return Result.FromSuccess();
    }

    public async Task<Result> RespondAsync(IGuildMemberRemove gatewayEvent, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        
        var key = ShardHelper.GetShardUserCountStatKey(_options.ShardIdentification!.ShardID);
        await db.StringDecrementAsync(key);
        
        return Result.FromSuccess();
    }

    public async Task<Result> RespondAsync(IGuildMembersChunk gatewayEvent, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        
        var key = ShardHelper.GetShardUserCountStatKey(_options.ShardIdentification!.ShardID);
        
        if (gatewayEvent.ChunkIndex is 0)
        {
            var guildResult = await _cache.TryGetValueAsync<IGuild>(KeyHelpers.CreateGuildCacheKey(gatewayEvent.GuildID), ct);
            

            if (!guildResult.IsDefined(out var guild))
                return Result.FromSuccess(); // ??

            if (guild.Members.IsDefined(out var members) && members.Count > 1)
                await db.StringDecrementAsync(key, members.Count);
        }
        
        await db.StringIncrementAsync(key, gatewayEvent.Members.Count);
        
        return Result.FromSuccess();
    }
}