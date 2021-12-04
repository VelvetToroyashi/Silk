using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Caching;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Silk.Core.Responders
{
    public class LateCacheSnapshotRespodner :
        IResponder<IChannelUpdate>,
        IResponder<IMessageUpdate>,
        IResponder<IChannelDelete>,
        IResponder<IGuildMemberRemove>,
        IResponder<IMessageDelete>,
        IResponder<IMessageDeleteBulk>
    {
        private readonly IMemoryCache _cache;
        public LateCacheSnapshotRespodner(IMemoryCache cache) => _cache = cache;
        
        public Task<Result> RespondAsync(IChannelUpdate gatewayEvent, CancellationToken ct = default)
        {
            var key = EarlyCacheSnapshotResponder.CacheKeyPrefix + KeyHelpers.CreateChannelCacheKey(gatewayEvent.ID);
            
            _cache.Remove(key);
            
            return Task.FromResult(Result.FromSuccess());
        }
        
        public Task<Result> RespondAsync(IMessageUpdate gatewayEvent, CancellationToken ct = default)
        {
            if (!gatewayEvent.GuildID.IsDefined(out var guildID))
                return Task.FromResult(Result.FromSuccess());
            
            if (!gatewayEvent.ID.IsDefined(out var messageID))
                return Task.FromResult(Result.FromSuccess());
            
            var key = EarlyCacheSnapshotResponder.CacheKeyPrefix + KeyHelpers.CreateMessageCacheKey(guildID, messageID);
            
            return Task.FromResult(Result.FromSuccess());
        }

        public Task<Result> RespondAsync(IChannelDelete gatewayEvent, CancellationToken ct = default)
        {
            var key = EarlyCacheSnapshotResponder.CacheKeyPrefix + KeyHelpers.CreateChannelCacheKey(gatewayEvent.ID);
            
            _cache.Remove(key);
            
            return Task.FromResult(Result.FromSuccess());
        }

        public Task<Result> RespondAsync(IGuildMemberRemove gatewayEvent, CancellationToken ct = default)
        {
            var key = EarlyCacheSnapshotResponder.CacheKeyPrefix + KeyHelpers.CreateUserCacheKey(gatewayEvent.User.ID);
            
            _cache.Remove(key);
            
            return Task.FromResult(Result.FromSuccess());
        }
        
        public Task<Result> RespondAsync(IMessageDelete gatewayEvent, CancellationToken ct = default)
        {
            if (!gatewayEvent.GuildID.IsDefined(out var guildID))
                return Task.FromResult(Result.FromSuccess());
            
            var key = EarlyCacheSnapshotResponder.CacheKeyPrefix + KeyHelpers.CreateMessageCacheKey(guildID, gatewayEvent.ID);
            
            _cache.Remove(key);
            
            return Task.FromResult(Result.FromSuccess());
        }

        public Task<Result> RespondAsync(IMessageDeleteBulk gatewayEvent, CancellationToken ct = default)
        {
            if (!gatewayEvent.GuildID.IsDefined())
                return Task.FromResult(Result.FromSuccess());
            
            foreach (var messageID in gatewayEvent.IDs)
            {
                var key = EarlyCacheSnapshotResponder.CacheKeyPrefix + KeyHelpers.CreateMessageCacheKey(gatewayEvent.ChannelID, messageID);
                
                _cache.Remove(key);
            }
            
            return Task.FromResult(Result.FromSuccess());
        }
    }
}