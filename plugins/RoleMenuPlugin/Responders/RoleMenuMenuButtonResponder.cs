using System;
using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace RoleMenuPlugin.Responders;

public class RoleMenuMenuButtonResponder : IResponder<IInteractionCreate>
{
    private readonly RoleMenuCreatorService _roleMenuCreatorService;
    public RoleMenuMenuButtonResponder(RoleMenuCreatorService roleMenuCreatorService)
    {
        _roleMenuCreatorService = roleMenuCreatorService;
    }

    public async Task<Result> RespondAsync(IInteractionCreate gatewayEvent, CancellationToken ct = default)
    {
        if (gatewayEvent.Type is not InteractionType.MessageComponent)
            return Result.FromSuccess();

        if (!gatewayEvent.GuildID.IsDefined(out var guildID) || !gatewayEvent.Member.IsDefined(out var member) || !member.User.IsDefined(out var user))
            return Result.FromSuccess();
        
        if (!gatewayEvent.Data.IsDefined(out var data)  ||
            !data.ComponentType.IsDefined() || 
            !data.CustomID.IsDefined())
            throw new InvalidOperationException("Component interaction without data?");

        if (!_roleMenuCreatorService.IsCreating(user.ID, guildID, out _))
            return Result.FromSuccess();

        var creationResult = await _roleMenuCreatorService.HandleInputAsync(user.ID, guildID, gatewayEvent);
        
        return creationResult.IsSuccess 
            ? Result.FromSuccess() 
            : Result.FromError(creationResult.Error!);
    }
}