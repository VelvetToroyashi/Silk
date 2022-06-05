using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Remora.Commands.Conditions;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;
using Remora.Results;
using RoleMenuPlugin.Database.MediatR;

namespace RoleMenuPlugin.Conditions;

public class RoleMenuCondition : ICondition<RoleMenuAttribute, IMessage>, ICondition<RoleMenuAttribute, Snowflake>
{
    private readonly IMediator _mediator;
    
    public RoleMenuCondition(IMediator mediator) => _mediator = mediator;

    public ValueTask<Result> CheckAsync(RoleMenuAttribute attribute, IMessage data, CancellationToken ct = default) => CheckAsync(attribute, data.ID, ct);
    
    public async ValueTask<Result> CheckAsync(RoleMenuAttribute attribute, Snowflake data, CancellationToken ct = default)
    {
        var existsResult = await _mediator.Send(new GetRoleMenu.Request(data.Value), ct);
        
        return existsResult.IsSuccess ? Result.FromSuccess() : Result.FromError(existsResult.Error);
    }
}