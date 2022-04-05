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

        var key = $"shard:{_options.ShardIdentification!.ShardID}:stats:guild_count";
        await db.StringSetAsync(key, (long)gatewayEvent.Guilds.Count);

        return Result.FromSuccess();
    }

    public async Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct = default)
    {
        if (gatewayEvent.IsUnavailable.IsDefined())
            return Result.FromSuccess();
        
        var db = _redis.GetDatabase();

        var key = $"shard:{_options.ShardIdentification!.ShardID}:stats:guild_count";
        await db.StringIncrementAsync(key);

        return Result.FromSuccess();
    }

    public async Task<Result> RespondAsync(IGuildDelete gatewayEvent, CancellationToken ct = default)
    {
        if (gatewayEvent.IsUnavailable.IsDefined())
            return Result.FromSuccess();
        
        var db = _redis.GetDatabase();
        
        var key = $"shard:{_options.ShardIdentification!.ShardID}:stats:guild_count";
        await db.StringDecrementAsync(key);
        
        return Result.FromSuccess();
    }

    public async Task<Result> RespondAsync(IGuildMemberAdd gatewayEvent, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        
        var key = $"shard:{_options.ShardIdentification!.ShardID}:stats:member_count";
        await db.StringIncrementAsync(key);
        
        return Result.FromSuccess();
    }

    public async Task<Result> RespondAsync(IGuildMemberRemove gatewayEvent, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        
        var key = $"shard:{_options.ShardIdentification!.ShardID}:stats:member_count";
        await db.StringDecrementAsync(key);
        
        return Result.FromSuccess();
    }

    public async Task<Result> RespondAsync(IGuildMembersChunk gatewayEvent, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        
        var key = $"shard:{_options.ShardIdentification!.ShardID}:stats:member_count";
        
        if (gatewayEvent.ChunkIndex is 0)
        {
            _ = _cache.TryGetValue<IGuild>(KeyHelpers.CreateGuildCacheKey(gatewayEvent.GuildID), out var guild);
            
            if (guild is null)
                return Result.FromSuccess(); // ??

            if (guild.Members.IsDefined(out var members) && members.Count > 1)
                await db.StringDecrementAsync(key, members.Count);
        }
        
        await db.StringIncrementAsync(key, gatewayEvent.Members.Count);
        
        return Result.FromSuccess();
    }
}