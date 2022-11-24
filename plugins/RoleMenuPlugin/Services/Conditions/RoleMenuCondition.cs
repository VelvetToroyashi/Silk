using System.Threading;
using System.Threading.Tasks;
using Remora.Commands.Conditions;
using Remora.Rest.Core;
using Remora.Results;
using RoleMenuPlugin.Database;
using IMessage = Remora.Discord.API.Abstractions.Objects.IMessage;

namespace RoleMenuPlugin.Conditions;

public class RoleMenuCondition : ICondition<RoleMenuAttribute, IMessage>, ICondition<RoleMenuAttribute, Snowflake>
{
    private readonly RoleMenuRepository _repo;
    
    public RoleMenuCondition(RoleMenuRepository repo) => _repo = repo;

    public ValueTask<Result> CheckAsync(RoleMenuAttribute attribute, IMessage data, CancellationToken ct = default) => CheckAsync(attribute, data.ID, ct);
    
    public async ValueTask<Result> CheckAsync(RoleMenuAttribute attribute, Snowflake data, CancellationToken ct = default)
    {
        var existsResult = await _repo.GetRoleMenuAsync(data.Value, ct);
        
        return (Result)existsResult;
    }
}