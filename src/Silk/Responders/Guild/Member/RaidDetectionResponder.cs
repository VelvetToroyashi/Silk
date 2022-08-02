using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Services.Guild;

namespace Silk.Responders;

public class RaidDetectionResponder : IResponder<IGuildMemberAdd>, IResponder<IMessageCreate>
{
    private readonly RaidDetectionService _raids;
    public RaidDetectionResponder(RaidDetectionService raids) => _raids = raids;

    public async Task<Result> RespondAsync(IGuildMemberAdd gatewayEvent, CancellationToken ct = default)
    {
        return await _raids.HandleJoinAsync(gatewayEvent.GuildID, gatewayEvent.User.Value.ID, gatewayEvent.JoinedAt, gatewayEvent.User.Value.Avatar is not null);
    }
    
    public async Task<Result> RespondAsync(IMessageCreate gatewayEvent, CancellationToken ct = default)
    {
        if (!gatewayEvent.GuildID.IsDefined(out var gid))
            return Result.FromSuccess();

        return await _raids.HandleMessageAsync(gid, gatewayEvent.ChannelID, gatewayEvent.ID, gatewayEvent.Author.ID, gatewayEvent.Content);
    }
}