using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Data.MediatR.Users;
using Silk.Data.MediatR.Users.History;

namespace Silk.Responders;

[ResponderGroup(ResponderGroup.Early)]
public class MemberDataCacherResponder : IResponder<IGuildMemberAdd>, IResponder<IGuildMemberRemove>, IResponder<IGuildMembersChunk>
{
    private readonly IMediator _mediator;
    public MemberDataCacherResponder(IMediator mediator) => _mediator = mediator;

    public async Task<Result> RespondAsync(IGuildMemberAdd gatewayEvent, CancellationToken ct = default)
    {
        var cacheResult = await _mediator.Send(new GetOrCreateUser.Request(gatewayEvent.GuildID, gatewayEvent.User.Value.ID, JoinedAt: gatewayEvent.JoinedAt), ct);

        if (cacheResult.IsDefined(out var user) && user.History.Last().JoinDate != gatewayEvent.JoinedAt)
            await _mediator.Send(new AddUserJoinDate.Request(gatewayEvent.GuildID, user.ID, gatewayEvent.JoinedAt), ct);

        return cacheResult.IsSuccess ? Result.FromSuccess() : Result.FromError(cacheResult.Error);
    }

    public async Task<Result> RespondAsync(IGuildMemberRemove gatewayEvent, CancellationToken ct = default)
    {
        var cacheResult = await _mediator.Send(new GetOrCreateUser.Request(gatewayEvent.GuildID, gatewayEvent.User.ID, JoinedAt: DateTimeOffset.MinValue), ct);
        
        if (cacheResult.IsDefined(out var user) && user.History.Last().LeaveDate is null)
            await _mediator.Send(new AddUserLeaveDate.Request(gatewayEvent.GuildID, user.ID, DateTimeOffset.UtcNow), ct);
        
        return cacheResult.IsSuccess ? Result.FromSuccess() : Result.FromError(cacheResult.Error);
    }
    
    public async Task<Result> RespondAsync(IGuildMembersChunk gatewayEvent, CancellationToken ct = default)
    {
        return Result.FromSuccess();
    }
}