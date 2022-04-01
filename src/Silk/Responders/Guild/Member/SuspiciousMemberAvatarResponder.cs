using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Services.Guild;

namespace Silk.Responders;

[ResponderGroup(ResponderGroup.Early)]
public class SuspiciousMemberAvatarResponder : IResponder<IGuildMemberAdd>, IResponder<IGuildMemberUpdate>
{
    private readonly PhishingAvatarDetectionService _avatarService;
    public SuspiciousMemberAvatarResponder(PhishingAvatarDetectionService avatarService)
    {
        _avatarService = avatarService;
    }

    public Task<Result> RespondAsync(IGuildMemberAdd    gatewayEvent, CancellationToken ct = default) 
        => _avatarService.CheckAvatarAsync(gatewayEvent);
    
    public Task<Result> RespondAsync(IGuildMemberUpdate gatewayEvent, CancellationToken ct = default)
        => _avatarService.CheckAvatarAsync(gatewayEvent);
}