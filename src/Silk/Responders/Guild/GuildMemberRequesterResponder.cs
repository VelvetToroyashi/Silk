using System;
using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Gateway.Commands;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Silk.Responders;

public class GuildMemberRequesterResponder : IResponder<IGuildCreate>
{
    private static readonly SemaphoreSlim  _sync = new(1);
    private static          DateTimeOffset _last = DateTimeOffset.UtcNow;

    private static readonly TimeSpan _minimum = TimeSpan.FromMilliseconds(500);
    
    private readonly DiscordGatewayClient _client;
    public GuildMemberRequesterResponder(DiscordGatewayClient client) => _client = client;

    public async Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct = default)
    {
        if (gatewayEvent.IsUnavailable.IsDefined(out var unavailable) && unavailable)
            return Result.FromSuccess(); // Thanks, Night.

        await _sync.WaitAsync(ct);

        var now = _last;
        _last = DateTimeOffset.UtcNow;

        if (now - _last < _minimum)
            await Task.Delay(_minimum - (now - _last), ct);
        
        _client.SubmitCommand(new RequestGuildMembers(gatewayEvent.ID, limit:0));

        _sync.Release();
        
        return Result.FromSuccess();
    }
}