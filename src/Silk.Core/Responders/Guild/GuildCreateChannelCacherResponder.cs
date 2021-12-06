using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Caching;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Silk.Core.Responders
{
    public class GuildCreateChannelCacherResponder : IResponder<IGuildCreate>
    {
        private readonly IMemoryCache _cache;
        public GuildCreateChannelCacherResponder(IMemoryCache cache) => _cache = cache;

        public Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct = default)
        {
            if (gatewayEvent.Channels.IsDefined(out var channels))
            {
                foreach (var channel in channels)
                {
                    var key = KeyHelpers.CreateChannelCacheKey(channel.ID);
                    _cache.Set(key, channel);
                }
            }
            
            return Task.FromResult(Result.FromSuccess());
        }
    }
}