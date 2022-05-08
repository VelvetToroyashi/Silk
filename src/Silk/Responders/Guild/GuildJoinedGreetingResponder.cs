using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Services.Data;
using Silk.Shared.Types;
using Silk.Utilities;
using StackExchange.Redis;

namespace Silk.Responders;

public class GuildJoinedGreetingResponder : IResponder<IGuildCreate>
{
    private readonly GuildCacherService _guildCacherService;
    
    public GuildJoinedGreetingResponder(GuildCacherService guildCacherService)
    {
        _guildCacherService = guildCacherService;
    }

    public async Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct = default)
    {
        if (gatewayEvent.IsUnavailable.IsDefined(out var unavailable) && unavailable)
            return Result.FromSuccess(); //Discord sometime sends unavailable guilds, we don't want to cache them
        
        return await _guildCacherService.GreetGuildAsync(gatewayEvent);
    }
}