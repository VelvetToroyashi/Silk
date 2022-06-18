using System;
using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace RoleMenuPlugin.Responders;

public class RoleMenuComponentResponder : IResponder<IInteractionCreate>
{
    private readonly RoleMenuService _roleMenus;
    public RoleMenuComponentResponder(RoleMenuService roleMenus) => _roleMenus = roleMenus;

    public async Task<Result> RespondAsync(IInteractionCreate gatewayEvent, CancellationToken ct = default)
    {
        if (gatewayEvent.Type is not InteractionType.MessageComponent)
            return Result.FromSuccess();

        if (!gatewayEvent.Data.IsDefined(out var data) || !data.IsT1)
            throw new InvalidOperationException("Component interaction without data?");
        
        if (!data.AsT1.CustomID.Equals(RoleMenuService.RoleMenuButtonPrefix) && !data.AsT1.CustomID.Equals(RoleMenuService.RoleMenuDropdownPrefix))
            return Result.FromSuccess();

        return data.AsT1.ComponentType switch
        {
            ComponentType.Button     => await _roleMenus.HandleButtonAsync(gatewayEvent),
            ComponentType.SelectMenu => await _roleMenus.HandleDropdownAsync(gatewayEvent),
            _                        => Result.FromSuccess()
        };
    }
}