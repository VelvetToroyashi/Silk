using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Caching;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Services.Data;
using StackExchange.Redis;

namespace Silk.Responders;

public class GuildJoinedCacherResponder : IResponder<IGuildCreate>
{
    private readonly IConnectionMultiplexer _cache;
    private readonly GuildCacherService     _cacher;

    public GuildJoinedCacherResponder(IConnectionMultiplexer cache, GuildCacherService cacher)
    {
        _cache  = cache;
        _cacher = cacher;
    }

    public async Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct = default)
    {
        if (gatewayEvent.IsUnavailable.IsDefined(out var unavailable) && unavailable)
            return Result.FromSuccess(); //Discord sometime sends unavailable guilds, we don't want to cache them

        var db = _cache.GetDatabase();
        
        if (await db.KeyExistsAsync(KeyHelpers.CreateGuildCacheKey(gatewayEvent.ID)))
            return Result.FromSuccess(); //We already have this guild cached
        
        await _cacher.GreetGuildAsync(gatewayEvent);
        
        if (!gatewayEvent.Members.IsDefined())
            return Result.FromError(new InvalidOperationError("Guild did not contain any members."));

        return await _cacher.CacheGuildAsync(gatewayEvent.ID, gatewayEvent.Members.Value);
    }
}