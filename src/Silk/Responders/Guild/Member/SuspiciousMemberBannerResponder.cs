using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Services.Guild;

namespace Silk.Responders;

public class SuspiciousMemberBannerResponder : IResponder<IGuildMemberAdd>, IResponder<IGuildMemberUpdate>
{
    private readonly SuspiciousUserDetectionService _suspiciousUserDetectionService;
    public SuspiciousMemberBannerResponder(SuspiciousUserDetectionService suspiciousUserDetectionService)
    {
        _suspiciousUserDetectionService = suspiciousUserDetectionService;
    }

    public Task<Result> RespondAsync(IGuildMemberAdd    gatewayEvent, CancellationToken ct = default) 
        => _suspiciousUserDetectionService.HandleSuspiciousUserAsync(gatewayEvent.GuildID, gatewayEvent.User.Value);
    
    public Task<Result> RespondAsync(IGuildMemberUpdate gatewayEvent, CancellationToken ct = default)
        => _suspiciousUserDetectionService.HandleSuspiciousUserAsync(gatewayEvent.GuildID, gatewayEvent.User);
}