using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Caching;
using Remora.Discord.Caching.Services;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Silk.Responders;

/// <summary>
/// A simple responder that re-caches guilds by redacting members and recaching the guild.
/// </summary>
public class GuildRecacherResponder : IResponder<IGuildCreate>
{
    private readonly CacheService _cacheService;
    public GuildRecacherResponder(CacheService cacheService) => _cacheService = cacheService;

    public Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct = default)
    {
        var guildKey = KeyHelpers.CreateGuildCacheKey(gatewayEvent.ID);
        
        _cacheService.Evict<IGuild>(guildKey);
        _cacheService.Cache<IGuild>(guildKey, (gatewayEvent as Guild)! with { Members = default });
        
        return Task.FromResult(Result.FromSuccess());
    }
}