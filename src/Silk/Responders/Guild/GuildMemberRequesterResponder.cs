using System;
using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Gateway.Commands;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Serilog;

namespace Silk.Responders;


public class A : IResponder<IGatewayEvent>
{

    public async Task<Result> RespondAsync(IGatewayEvent gatewayEvent, CancellationToken ct = default)
    {
        if (gatewayEvent is not IGuildMembersChunk gmc)
            Log.Information("Received event: {EventType}", gatewayEvent.GetType().Name);
        else 
            Log.Information("Received event: {EventName} ({Chunk}/{Chunks} {Members})", gatewayEvent.GetType().Name, gmc.ChunkIndex + 1, gmc.ChunkCount, gmc.Members.Count);
        
        return Result.FromSuccess();
    }
}

public class GuildMemberRequesterResponder : IResponder<IGuildCreate>//, IResponder<IGatewayEvent>

{
    private static readonly SemaphoreSlim  _sync = new(1);
    private static          DateTimeOffset _last = DateTimeOffset.UtcNow;

    private static readonly TimeSpan _minimumDelta = TimeSpan.FromMilliseconds(500);
    
    private readonly DiscordGatewayClient _client;
    public GuildMemberRequesterResponder(DiscordGatewayClient client) => _client = client;

    public async Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct = default)
    {
        if (gatewayEvent.IsUnavailable.IsDefined(out var unavailable) && unavailable)
            return Result.FromSuccess(); // Thanks, Night.
        
        await _sync.WaitAsync(ct);

        var now = _last;
        _last = DateTimeOffset.UtcNow;

        var delta = _last - now;

        if (delta < _minimumDelta)
            await Task.Delay(_minimumDelta - delta, ct);
        
        _client.SubmitCommand(new RequestGuildMembers(gatewayEvent.ID, limit: 5000));

        _sync.Release();
        
        return Result.FromSuccess();
    }
}