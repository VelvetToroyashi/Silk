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

public class MemberGreetingResponder : IResponder<IGuildMemberAdd>
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
}