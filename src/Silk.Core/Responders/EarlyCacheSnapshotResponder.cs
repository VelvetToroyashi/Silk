using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Caching;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Silk.Core.Responders
{
    /// <summary>
    /// A responder that re-caches data to allow it to be accessed during normal responders, and evicts them after normal responders are done.
    /// </summary>
    [PublicAPI]
    public class EarlyCacheSnapshotResponder :
        IResponder<IChannelCreate>,
        IResponder<IGuildMemberAdd>,
        IResponder<IMessageCreate>
    {
        public const string CacheKeyPrefix = "old_";
        
        private readonly IMemoryCache _cache;
        public EarlyCacheSnapshotResponder(IMemoryCache cache) => _cache = cache;

        public Task<Result> RespondAsync(IChannelCreate gatewayEvent, CancellationToken ct = default)
        {
            var key = CacheKeyPrefix + KeyHelpers.CreateChannelCacheKey(gatewayEvent.ID);

            _cache.Set(key, gatewayEvent);

            return Task.FromResult(Result.FromSuccess());
        }
        
        public Task<Result> RespondAsync(IGuildMemberAdd gatewayEvent, CancellationToken ct = default)
        {
            if (!gatewayEvent.User.IsDefined(out var user))
                return Task.FromResult(Result.FromSuccess());
            
            var key = CacheKeyPrefix + KeyHelpers.CreateUserCacheKey(user.ID);
            
            return Task.FromResult(Result.FromSuccess());
        }

        public Task<Result> RespondAsync(IMessageCreate gatewayEvent, CancellationToken ct = default)
        {
            var key = CacheKeyPrefix + KeyHelpers.CreateMessageCacheKey(gatewayEvent.ChannelID, gatewayEvent.ID);
            
            _cache.Set(key, gatewayEvent);
            
            return Task.FromResult(Result.FromSuccess());
        }
    }
}