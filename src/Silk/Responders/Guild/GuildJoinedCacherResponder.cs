using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Services.Data;
using Silk.Utilities;
using StackExchange.Redis;

namespace Silk.Responders;

[ResponderGroup(ResponderGroup.Early)]
public class GuildJoinedCacherResponder : IResponder<IGuildCreate>
{
    private readonly IConnectionMultiplexer _cache;
    private readonly GuildCacherService     _cacher;
    private readonly IShardIdentification   _shard;

    public GuildJoinedCacherResponder(IConnectionMultiplexer cache, GuildCacherService cacher, IShardIdentification shard)
    {
        _cache  = cache;
        _cacher = cacher;
        _shard  = shard;
    }

    public async Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct = default)
    {
        if (gatewayEvent.IsUnavailable.IsDefined(out var unavailable) && unavailable)
            return Result.FromSuccess(); //Discord sometime sends unavailable guilds, we don't want to cache them
        
        var db = _cache.GetDatabase();

        var shardKey = ShardHelper.GetShardGuildsKey(_shard.ShardID);
        
        var keyExists = await db.HashGetAsync(shardKey, gatewayEvent.ID.Value);

        if (keyExists.HasValue)
            return Result.FromSuccess();
        
        await db.HashSetAsync(shardKey, gatewayEvent.ID.Value, default(string?));

        await _cacher.GreetGuildAsync(gatewayEvent);
        
        await _cacher.CacheGuildAsync(gatewayEvent.ID);
        
        return Result.FromSuccess();
    }
}