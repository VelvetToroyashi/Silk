using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Caching;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Core.Data.Entities;
using Silk.Core.Services.Server;

namespace Silk.Core.Responders
{
    public class MemberGreetingResponder : IResponder<IGuildMemberAdd>, IResponder<IGuildMemberUpdate>, IResponder<IChannelUpdate>
    {
        private readonly IMemoryCache         _cache;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly GuildGreetingService _greetingService;
        
        public MemberGreetingResponder(IMemoryCache cache, IDiscordRestGuildAPI guildApi, GuildGreetingService greetingService)
        {
            _cache = cache;
            _guildApi = guildApi;
            _greetingService = greetingService;
        }

        public Task<Result> RespondAsync(IGuildMemberAdd gatewayEvent, CancellationToken ct = default)
            => _greetingService.QueueGreetingAsync(gatewayEvent.GuildID, gatewayEvent.User.Value.ID, GreetingOption.GreetOnJoin);

        public async Task<Result> RespondAsync(IGuildMemberUpdate gatewayEvent, CancellationToken ct = default)
        {
            var cacheKey = EarlyCacheSnapshotResponder.CacheKeyPrefix + KeyHelpers.CreateGuildMemberKey(gatewayEvent.GuildID, gatewayEvent.User.ID);
            
            if (!_cache.TryGetValue(cacheKey, out IGuildMember member))
            {
                var memberRes = await _guildApi.GetGuildMemberAsync(gatewayEvent.GuildID, gatewayEvent.User.ID, ct);
                
                if (!memberRes.IsDefined(out member!))
                    return Result.FromError(memberRes.Error!);

                if (!await _greetingService.HasGreetingRoleAsync(gatewayEvent.GuildID, member))
                    return Result.FromSuccess(); // No greeting role, no greeting.
                
                return await _greetingService.QueueGreetingAsync(gatewayEvent.GuildID, gatewayEvent.User.ID, GreetingOption.GreetOnJoin);
            }
            else
            {
                if (member.Roles.Count <= gatewayEvent.Roles.Count)
                    return Result.FromSuccess(); // Same or less roles, therefore we won't even attempt to greet.

                if (!await _greetingService.HasGreetingRoleAsync(gatewayEvent.GuildID, member))
                    return Result.FromSuccess(); // No greeting role, therefore we won't even attempt to greet.

                return await _greetingService.QueueGreetingAsync(gatewayEvent.GuildID, gatewayEvent.User.ID, GreetingOption.GreetOnRole);
            }
        }

        public async Task<Result> RespondAsync(IChannelUpdate gatewayEvent, CancellationToken ct = default)
        {
            if (!gatewayEvent.GuildID.IsDefined(out var guildID))
                return Result.FromSuccess();

            if (!gatewayEvent.PermissionOverwrites.IsDefined(out var permissions))
                return Result.FromSuccess();
            
            var cacheKey = EarlyCacheSnapshotResponder.CacheKeyPrefix + KeyHelpers.CreateChannelCacheKey(gatewayEvent.ID);

            if (_cache.TryGetValue(cacheKey, out IChannel oldChannel))
            {
                if (oldChannel.PermissionOverwrites.Value.Count >= permissions.Count)
                    return Result.FromSuccess(); // Same or less permissions, therefore we won't even attempt to greet.

                var newPermissions =
                    permissions
                       .Where(p => p.Type is PermissionOverwriteType.Member)
                       .Union(oldChannel.PermissionOverwrites.Value.Where(p => p.Type is PermissionOverwriteType.Member))
                       .Distinct()
                       .FirstOrDefault(); // If there's multiple differences, cache is fucked. ~Velvet
                
                if (newPermissions is null)
                    return Result.FromSuccess();
                
                var memberRes = await _guildApi.GetGuildMemberAsync(guildID, newPermissions.ID, ct);
                
                if (!memberRes.IsDefined(out var member))
                    return Result.FromError(memberRes.Error!);

                if (!await _greetingService.CanAccessGreetingChannelAsync(guildID, gatewayEvent, member))
                    return Result.FromSuccess(); // No greeting channel, no greeting.
                
                return await _greetingService.QueueGreetingAsync(guildID, member.User.Value.ID, GreetingOption.GreetOnJoin);
            }

            return Result.FromSuccess(); // Channel wasn't in cache, so we can't determine which overwrites to check.
        }
    }
}