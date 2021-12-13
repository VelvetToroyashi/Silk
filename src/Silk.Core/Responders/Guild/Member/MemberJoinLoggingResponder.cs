using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Core.Services.Server;

namespace Silk.Core.Responders;

public class MemberJoinLoggingResponder : IResponder<IGuildMemberAdd>
{
    private readonly MemberLoggerService _memberLoggerService;
    
    public MemberJoinLoggingResponder(MemberLoggerService memberLoggerService) 
        => _memberLoggerService = memberLoggerService;

    public Task<Result> RespondAsync(IGuildMemberAdd gatewayEvent, CancellationToken ct = default)
        => _memberLoggerService.LogMemberJoinAsync(gatewayEvent.GuildID, gatewayEvent);
}