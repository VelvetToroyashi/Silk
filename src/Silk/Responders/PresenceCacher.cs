using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Caching;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Silk.Responders;

public class PresenceCacher : IResponder<IPresenceUpdate>, IResponder<IGuildCreate>
{
    private readonly IMemoryCache _cacheService;
    public PresenceCacher(IMemoryCache cacheService) => _cacheService = cacheService;

    public async Task<Result> RespondAsync(IPresenceUpdate gatewayEvent, CancellationToken ct = default)
    {
        _cacheService.Set(KeyHelpers.CreatePresenceCacheKey(default, gatewayEvent.User.ID.Value), (IPartialPresence)gatewayEvent);

        return Result.FromSuccess();
    }

    public async Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct = default)
    {
        if (!gatewayEvent.Presences.IsDefined(out var presences))
            return Result.FromSuccess();
        
        foreach (var presence in presences)
            _cacheService.Set(KeyHelpers.CreatePresenceCacheKey(default, presence.User.Value.ID.Value), presence);
        
        return Result.FromSuccess();
    }
}