using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Services.Guild;

namespace Silk.Responders;

public class MemberLoggingResponder : IResponder<IGuildMemberAdd>, IResponder<IGuildMemberRemove>
{
    private readonly MemberLoggerService _memberLoggerService;
    
    public MemberLoggingResponder(MemberLoggerService memberLoggerService) 
        => _memberLoggerService = memberLoggerService;

    public Task<Result> RespondAsync(IGuildMemberAdd gatewayEvent, CancellationToken ct = default)
        => _memberLoggerService.LogMemberJoinAsync(gatewayEvent.GuildID, gatewayEvent);

    public Task<Result> RespondAsync(IGuildMemberRemove gatewayEvent, CancellationToken ct = default)
        => _memberLoggerService.LogMemberLeaveAsync(gatewayEvent.GuildID, gatewayEvent.User);
}