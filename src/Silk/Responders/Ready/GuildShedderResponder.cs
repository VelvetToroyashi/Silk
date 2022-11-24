using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Data.MediatR.Guilds;

namespace Silk.Responders;

public class GuildShedderResponder : IResponder<IReady>
{
    private readonly IMediator                      _mediator;
    private readonly IShardIdentification           _shard;
    private readonly ILogger<GuildShedderResponder> _logger;
   
    
    public GuildShedderResponder(IMediator mediator, IShardIdentification shard, ILogger<GuildShedderResponder> logger)
    {
        _mediator = mediator;
        _shard    = shard;
        _logger   = logger;
    }

    public async Task<Result> RespondAsync(IReady gatewayEvent, CancellationToken ct = default)
    {
        _logger.LogInformation("Preparring to shed guilds...");

        var shed = await _mediator.Send(new ShedGuilds.Request(_shard.ShardID, _shard.ShardCount, gatewayEvent.Guilds.Select(g => g.ID).ToArray()), ct);
        
        if (shed.IsSuccess)
            _logger.LogInformation("Successfully shed {ShedCount} guilds.", shed.Entity);
        else 
            _logger.LogError("Failed to shed guilds: {Error}", shed.Error);

        return Result.FromSuccess();
    }
}