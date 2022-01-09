using System;
using System.Text.RegularExpressions;
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

        if (!gatewayEvent.Data.IsDefined(out var data)  ||
            !data.ComponentType.IsDefined(out var type) || 
            !data.CustomID.IsDefined(out var customID))
            throw new InvalidOperationException("Component interaction without data?");
        
        if (!customID.StartsWith(RoleMenuService.RoleMenuButtonPrefix))
            return Result.FromSuccess();

        if (type is ComponentType.Button)
            return await _roleMenus.HandleButtonAsync(gatewayEvent);
        
        return Result.FromSuccess();
    }
}