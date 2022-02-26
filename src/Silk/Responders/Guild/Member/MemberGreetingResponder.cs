using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Caching;
using Remora.Discord.Caching.Services;
using Remora.Discord.Gateway.Responders;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Services.Guild;

namespace Silk.Responders;

public class MemberGreetingResponder : IResponder<IGuildMemberAdd>, IResponder<IGuildMemberUpdate>
{
    private readonly CacheService         _cache;
    private readonly GuildGreetingService _greetingService;

    public MemberGreetingResponder(CacheService cache, GuildGreetingService greetingService)
    {
        _cache           = cache;
        _greetingService = greetingService;
    }

    public Task<Result> RespondAsync(IGuildMemberAdd gatewayEvent, CancellationToken ct = default)
        => _greetingService.TryGreetMemberAsync(gatewayEvent.GuildID, gatewayEvent.User.Value, GreetingOption.GreetOnJoin);

    public Task<Result> RespondAsync(IGuildMemberUpdate gatewayEvent, CancellationToken ct = default)
    {
        if (!_cache.TryGetPreviousValue(KeyHelpers.CreateGuildMemberKey(gatewayEvent.GuildID, gatewayEvent.User.ID), out IGuildMember? guildMember))
            return Task.FromResult(Result.FromSuccess());

        if (guildMember.Roles.Count <= gatewayEvent.Roles.Count)
            return Task.FromResult(Result.FromSuccess());

        return _greetingService.TryGreetMemberAsync(gatewayEvent.GuildID, gatewayEvent.User, GreetingOption.GreetOnRole);
    }
}