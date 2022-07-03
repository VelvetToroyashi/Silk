using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Gateway.Responders;
using Remora.Discord.Interactivity;
using Remora.Results;

namespace RoleMenuPlugin;

public class RoleMenuButtonFixer : IResponder<IInteractionCreate>
{
    private readonly IDiscordRestChannelAPI     _channels;
    private readonly IDiscordRestInteractionAPI _interactions;
    
    public RoleMenuButtonFixer(IDiscordRestChannelAPI channels, IDiscordRestInteractionAPI interactions)
    {
        _channels     = channels;
        _interactions = interactions;
    }

    public async Task<Result> RespondAsync(IInteractionCreate gatewayEvent, CancellationToken ct)
    {
        if (gatewayEvent.Type is not  InteractionType.MessageComponent)
            return Result.FromSuccess();
        
        if (gatewayEvent.Data.Value.Value is not IMessageComponentData data || data.CustomID is not RoleMenuInteractionCommands.RoleMenuButtonPrefix)
            return Result.FromSuccess();

        var message = gatewayEvent.Message.Value;

        await _channels.EditMessageAsync(message.ChannelID, message.ID, message.Content, components: new[] { new ActionRowComponent(new[]
        {
            new ButtonComponent(ButtonComponentStyle.Success, "Get Roles!", CustomID: CustomIDHelpers.CreateButtonID(RoleMenuInteractionCommands.RoleMenuButtonPrefix))
        })}, ct: ct);

        await _interactions.CreateInteractionResponseAsync(gatewayEvent.ID, gatewayEvent.Token, new InteractionResponse(InteractionCallbackType.DeferredUpdateMessage), ct: ct);
        
        await _interactions.CreateFollowupMessageAsync
        (
         gatewayEvent.ApplicationID,
         gatewayEvent.Token,
         "This role-menu has been retroactively repaired to work with the new system. \n" +
         "Please use the button again to activate the role-menu.",
         flags: MessageFlags.Ephemeral,
         ct: ct
        );
        
        return Result.FromSuccess();
    }
}