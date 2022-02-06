using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Services.Data;
using Silk.Shared.Types;

namespace Silk.Responders;

public class GuildJoinedCacherResponder : IResponder<IGuildCreate>
{
    private readonly IMemoryCache _cache;
    private readonly GuildCacherService _guildCacherService;
    
    public GuildJoinedCacherResponder(IMemoryCache cache, GuildCacherService guildCacherService)
    {
        _cache              = cache;
        _guildCacherService = guildCacherService;
    }

    public async Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct = default)
    {
        if (gatewayEvent.IsUnavailable.IsDefined(out var unavailable) && unavailable)
            return Result.FromSuccess(); //Discord sometime sends unavailable guilds, we don't want to cache them

        if (_cache.TryGetValue(SilkKeyHelper.GenerateGuildIdentifierKey(gatewayEvent.ID), out _))
            return Result.FromSuccess(); //We already have this guild cached
        
        await _guildCacherService.GreetGuildAsync(gatewayEvent);
        
        if (!gatewayEvent.Members.IsDefined())
            return Result.FromError(new InvalidOperationError("Guild did not contain any members."));

        return await _guildCacherService.CacheGuildAsync(gatewayEvent.ID, gatewayEvent.Members.Value);
    }
}