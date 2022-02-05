using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Shared.Types;

namespace Silk.Responders;

public class ReadyGuildCountCacher : IResponder<IReady>
{
    private readonly IMemoryCache _cache;

    public ReadyGuildCountCacher(IMemoryCache cache) => _cache = cache;

    public Task<Result> RespondAsync(IReady gatewayEvent, CancellationToken ct = default)
    {
        var key = SilkKeyHelper.GenerateGuildCountKey();

        _cache.Set(key, gatewayEvent.Guilds.Count, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = null });
        
        return Task.FromResult(Result.FromSuccess());
    }
}