using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Shared.Types;
using Silk.Utilities;
using StackExchange.Redis;

namespace Silk.Responders;

public class ReadyGuildCountCacher : IResponder<IReady>
{
    private readonly IConnectionMultiplexer _cache;
    private readonly IShardIdentification _shard;

    public ReadyGuildCountCacher(IConnectionMultiplexer cache, IShardIdentification shard)
    {
        _cache = cache;
        _shard = shard;
    }

    public Task<Result> RespondAsync(IReady gatewayEvent, CancellationToken ct = default)
    {
        var key = ShardHelper.GetShardGuildCountStatKey(_shard.ShardID);

        _cache.GetDatabase().StringSetAsync(key, gatewayEvent.Guilds.Count);
        
        return Task.FromResult(Result.FromSuccess());
    }
}