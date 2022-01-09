using System.Drawing;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Gateway.Responders;
using Remora.Plugins;
using Remora.Results;

namespace TestPlugin;

public class ButtonCommand : CommandGroup
{
    private readonly ICommandContext        _context;
    private readonly IDiscordRestChannelAPI _channels;
    
    public ButtonCommand(ICommandContext context, IDiscordRestChannelAPI channels)
    {
        _context         = context;
        _channels = channels;
    }
    
    [Command("sb")]
    public Task<Result<IMessage>> ShowButton() 
        => _channels
           .CreateMessageAsync
                (
                 _context.ChannelID,
                 "\u200b",
                 components: new IMessageComponent[]
                 {
                     new ActionRowComponent(new IMessageComponent[]
                     {
                         new ButtonComponent(ButtonComponentStyle.Primary, "Click Me!", CustomID: "clickme")
                     })
                 });
}

public class ButtonResponder : IResponder<IInteractionCreate>
{
    private readonly IDiscordRestInteractionAPI _interactions;
    public ButtonResponder(IDiscordRestInteractionAPI interactions) => _interactions = interactions;

    public async Task<Result> RespondAsync(IInteractionCreate gatewayEvent, CancellationToken ct = default)
    {
        if (!gatewayEvent.Data.IsDefined(out var data))
            throw new ArgumentException("Data is not defined");

        if (!data.ComponentType.IsDefined(out var type) || type is not ComponentType.Button)
            return Result.FromSuccess();
        
        if (!data.CustomID.IsDefined(out var ID) || ID != "clickme")
            return Result.FromSuccess();

        await _interactions.CreateInteractionResponseAsync(gatewayEvent.ID, gatewayEvent.Token, new InteractionResponse(InteractionCallbackType.DeferredUpdateMessage));

        await _interactions.DeleteOriginalInteractionResponseAsync(gatewayEvent.ApplicationID, gatewayEvent.Token);
        
        var res = await _interactions.CreateFollowupMessageAsync(gatewayEvent.ApplicationID, gatewayEvent.Token, "You clicked the button!");
        
        return res.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(res.Error);
    }
}