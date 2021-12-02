using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Caching;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Silk.Core.Responders
{
    /// <summary>
    ///     Caches the current user data based on the <see cref="IReady" /> event data.
    /// </summary>
    public class ReadyUserCacher : IResponder<IReady>
    {
        private readonly IMemoryCache _cache;
        public ReadyUserCacher(IMemoryCache cache) => _cache = cache;

        public async Task<Result> RespondAsync(IReady gatewayEvent, CancellationToken ct = default)
        {
            IUser currentUserObject = gatewayEvent.User;
            object currentUserCacheKey = KeyHelpers.CreateCurrentUserCacheKey();

            _cache.Set(currentUserCacheKey, currentUserObject, new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove });

            return Result.FromSuccess();
        }
    }
}