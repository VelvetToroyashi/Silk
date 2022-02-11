using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Caching;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Silk.Responders;

/// <summary>
///     Caches the current user data based on the <see cref="IReady" /> event data.
/// </summary>
public class ReadyUserCacher : IResponder<IReady>
{
    private readonly IMemoryCache _cache;
    public ReadyUserCacher(IMemoryCache cache) => _cache = cache;

    public async Task<Result> RespondAsync(IReady gatewayEvent, CancellationToken ct = default)
    {
        var  currentUserObject   = gatewayEvent.User;
        var currentUserCacheKey = KeyHelpers.CreateCurrentUserCacheKey();

        _cache.Set(currentUserCacheKey, currentUserObject, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = null });

        return Result.FromSuccess();
    }
}