using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Caching;
using Remora.Discord.Caching.Services;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Silk.Responders;

public class MemberChunkedHandler : IResponder<IGuildMembersChunk>
{
    private readonly CacheService _cache;
    public MemberChunkedHandler(CacheService cache)
    {
        _cache = cache;
    }

    public async Task<Result> RespondAsync(IGuildMembersChunk gatewayEvent, CancellationToken ct)
    {
        var key = KeyHelpers.CreateGuildMembersKey(gatewayEvent.GuildID, default, default);

        var fetchResult = await _cache.TryGetValueAsync<IReadOnlyList<IGuildMember>>(key, ct);

        if (gatewayEvent.ChunkIndex is 0 || !fetchResult.IsSuccess)
        {
            await _cache.CacheAsync(key, gatewayEvent.Members, ct);
        }
        else
        {
            var newMembers = fetchResult.Entity.Concat(gatewayEvent.Members);

            await _cache.CacheAsync(key, newMembers, ct);
        }
        
        return Result.FromSuccess();
    }
}