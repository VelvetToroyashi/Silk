using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Services.Guild;

namespace Silk.Core.Remora.Services;

public class AntiInviteResponder : IResponder<IMessageCreate>
{
    private readonly InviteDectectionService _invites;
    public AntiInviteResponder(InviteDectectionService invites) => _invites = invites;

    public Task<Result> RespondAsync(IMessageCreate gatewayEvent, CancellationToken ct = default)
        => _invites.CheckForInviteAsync(gatewayEvent);
}