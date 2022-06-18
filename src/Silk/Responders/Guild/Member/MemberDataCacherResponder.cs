using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Gateway.Responders;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.MediatR.Users;
using Silk.Data.MediatR.Users.History;
using Silk.Services.Data;

namespace Silk.Responders;

[ResponderGroup(ResponderGroup.Early)]
public class MemberDataCacherResponder : IResponder<IReady>, IResponder<IGuildMemberAdd>, IResponder<IGuildMemberRemove>, IResponder<IGuildMembersChunk>
{
    private static readonly ConcurrentDictionary<Snowflake, List<IGuildMember>> _queue = new();

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

    public Task<Result> RespondAsync(IGuildMembersChunk gatewayEvent, CancellationToken ct = default)
    {
        if (gatewayEvent.Nonce.IsDefined())
            return Task.FromResult(Result.FromSuccess());

        if (gatewayEvent.ChunkIndex + 1 < gatewayEvent.ChunkCount)
        {
            _queue.GetOrAdd(gatewayEvent.GuildID, _ => new()).AddRange(gatewayEvent.Members);
            return Task.FromResult(Result.FromSuccess());
        }

        if (!_queue.TryRemove(gatewayEvent.GuildID, out var members))
        {
            // We've received them out of order. Drat.
            return Task.FromResult(Result.FromSuccess());
        }

        return _cacher.CacheMembersAsync(gatewayEvent.GuildID, members);
    }

    public Task<Result> RespondAsync(IReady gatewayEvent, CancellationToken ct = default)
    {
        _queue.Clear();

        foreach (var guild in gatewayEvent.Guilds)
            _queue[guild.ID] = new();

        return Task.FromResult(Result.FromSuccess());
    }
}