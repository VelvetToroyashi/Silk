using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
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
        if (gatewayEvent.Guild.IsT1)
            return Result.FromSuccess(); //Discord sometime sends unavailable guilds, we don't want to cache them
        
        var guild = gatewayEvent.Guild.AsT0;
        var db = _cache.GetDatabase();

        var shardKey = ShardHelper.GetShardGuildsKey(_shard.ShardID);
        
        var keyExists = await db.HashGetAsync(shardKey, guild.ID.Value);

        if (keyExists.HasValue)
            return Result.FromSuccess();
        
        await db.HashSetAsync(shardKey, guild.ID.Value, default(string?));

        await _cacher.GreetGuildAsync(guild);
        
        await _cacher.CacheGuildAsync(guild.ID);
        
        return Result.FromSuccess();
    }
}