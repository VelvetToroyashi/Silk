using System;
using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Services.Guild;

namespace Silk.Responders;

[ResponderGroup(ResponderGroup.Early)]
public class SuspiciousMemberUsernameResponder : IResponder<IGuildMemberAdd>, IResponder<IGuildMemberUpdate>
{
    private readonly PhishingDetectionService _suspiciousUserDetectionService;
    
    public SuspiciousMemberUsernameResponder(PhishingDetectionService suspiciousUserDetectionService)
        => _suspiciousUserDetectionService = suspiciousUserDetectionService;

    public Task<Result> RespondAsync(IGuildMemberAdd gatewayEvent, CancellationToken ct = default) 
        => _suspiciousUserDetectionService.HandlePotentialSuspiciousUsernameAsync(gatewayEvent.GuildID, gatewayEvent.User.Value, true);
    
    public Task<Result> RespondAsync(IGuildMemberUpdate gatewayEvent, CancellationToken ct = default)
        => gatewayEvent.JoinedAt > DateTimeOffset.UtcNow.Subtract(TimeSpan.FromSeconds(15)) 
            ? Task.FromResult(Result.FromSuccess()) 
            :_suspiciousUserDetectionService.HandlePotentialSuspiciousUsernameAsync(gatewayEvent.GuildID, gatewayEvent.User, false);
}