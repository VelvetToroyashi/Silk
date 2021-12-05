using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Caching;
using Remora.Discord.Caching.Services;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Core.Data.Entities;
using Silk.Core.Services.Server;

namespace Silk.Core.Responders
{
    public class MemberGreetingResponder : IResponder<IGuildMemberAdd>, IResponder<IGuildMemberUpdate>, IResponder<IChannelUpdate>
    {
        private readonly CacheService         _cache;
        private readonly GuildGreetingService _greetingService;
        
        public MemberGreetingResponder(CacheService cache, GuildGreetingService greetingService)
        {
            _cache = cache;
            _greetingService = greetingService;
        }

        public Task<Result> RespondAsync(IGuildMemberAdd gatewayEvent, CancellationToken ct = default)
            => _greetingService.TryGreetMemberAsync(gatewayEvent.GuildID, gatewayEvent.User.Value, GreetingOption.GreetOnJoin);

        public Task<Result> RespondAsync(IGuildMemberUpdate gatewayEvent, CancellationToken ct = default)
            => _greetingService.TryGreetMemberAsync(gatewayEvent.GuildID, gatewayEvent.User, GreetingOption.GreetOnRole);

        public Task<Result> RespondAsync(IChannelUpdate gatewayEvent, CancellationToken ct = default)
        {
            if (!gatewayEvent.GuildID.IsDefined(out var guildID))
                return Task.FromResult(Result.FromSuccess());
            
            var cacheKey = KeyHelpers.CreateChannelCacheKey(gatewayEvent.ID);

            if (_cache.TryGetValue(cacheKey, out IChannel? newChannel) &&
                _cache.TryGetPreviousValue(cacheKey, out IChannel? oldChannel))
                return _greetingService.TryGreetAsync(guildID, oldChannel, newChannel);

            return Task.FromResult(Result.FromSuccess()); // Channel wasn't in cache, so we can't determine which overwrites to check.
        }
    }
}