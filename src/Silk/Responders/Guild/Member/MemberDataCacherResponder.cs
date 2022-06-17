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
using Silk.Services.Data;

namespace Silk.Responders;

[ResponderGroup(ResponderGroup.Early)]
public class MemberDataCacherResponder : IResponder<IGuildMemberAdd>, IResponder<IGuildMemberRemove>, IResponder<IGuildMembersChunk>
{
    private readonly IMediator          _mediator;
    private readonly GuildCacherService _cacher;
    public MemberDataCacherResponder(IMediator mediator, GuildCacherService cacher)
    {
        _mediator = mediator;
        _cacher   = cacher;
    }

    public async Task<Result> RespondAsync(IGuildMemberAdd gatewayEvent, CancellationToken ct = default)
    {
        var cacheResult = await _mediator.Send(new GetOrCreateUser.Request(gatewayEvent.GuildID, gatewayEvent.User.Value.ID, JoinedAt: gatewayEvent.JoinedAt), ct);

        if (cacheResult.IsDefined(out var user) && user.History.Last().JoinDate != gatewayEvent.JoinedAt)
            await _mediator.Send(new AddUserJoinDate.Request(gatewayEvent.GuildID, user.ID, gatewayEvent.JoinedAt), ct);

        return (Result)cacheResult;
    }

    public async Task<Result> RespondAsync(IGuildMemberRemove gatewayEvent, CancellationToken ct = default)
    {
        var cacheResult = await _mediator.Send(new GetOrCreateUser.Request(gatewayEvent.GuildID, gatewayEvent.User.ID, JoinedAt: DateTimeOffset.MinValue), ct);
        
        if (cacheResult.IsDefined(out var user) && user.History.Last().LeaveDate is null)
            await _mediator.Send(new AddUserLeaveDate.Request(gatewayEvent.GuildID, user.ID, DateTimeOffset.UtcNow), ct);
        
        return (Result)cacheResult;
    }

    public async Task<Result> RespondAsync(IGuildMembersChunk gatewayEvent, CancellationToken ct = default)
    {
        _ = _cacher.CacheMembersAsync(gatewayEvent.GuildID, gatewayEvent.Members);

        return Result.FromSuccess();
    }
}