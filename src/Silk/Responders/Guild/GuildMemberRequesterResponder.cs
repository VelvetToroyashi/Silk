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
    private readonly DiscordGatewayClient _client;
    public GuildMemberRequesterResponder(DiscordGatewayClient client) => _client = client;

    public async Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken ct = default)
    {
        if (gatewayEvent.IsUnavailable.IsDefined(out var unavailable) && unavailable)
            return Result.FromSuccess(); // Thanks, Night.
        
        _client.SubmitCommand(new RequestGuildMembers(gatewayEvent.ID));
        
        return Result.FromSuccess();
    }
}