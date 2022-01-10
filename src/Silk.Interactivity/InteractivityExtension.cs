using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Results;

namespace Silk.Interactivity;

public class InteractivityExtension
{
    private readonly InteractivityWaiter<IMessageCreate>     _messageWaiter;
    private readonly InteractivityWaiter<IInteractionCreate> _interactionWaiter;
    
    public InteractivityExtension
    (
        InteractivityWaiter<IMessageCreate> messageWaiter,
        InteractivityWaiter<IInteractionCreate> interactionWaiter
    )
    {
        _messageWaiter     = messageWaiter;
        _interactionWaiter = interactionWaiter;
    }


    public Task<Result<IInteractionCreate?>> WaitForButtonAsync(IUser user, IMessage message)
        => WaitForButtonAsync(ev => 
                                  ev.User.IsDefined(out var evUser) && evUser.ID == user.ID &&
                                  ev.Message.IsDefined(out var evMessage) && evMessage.ID == message.ID
                             );

    public Task<Result<IInteractionCreate?>> WaitForButtonAsync(Func<IInteractionCreate, bool> predicate)
        => _interactionWaiter.WaitForEventAsync(predicate);

    public Task<Result<IMessageCreate?>> WaitForMessageAsync(IUser user)
        => _messageWaiter.WaitForEventAsync(ev => ev.Author.ID == user.ID);

    public Task<Result<IMessageCreate?>> WaitForMessageAsync(Func<IMessageCreate, bool> predicate)
        => _messageWaiter.WaitForEventAsync(predicate);
    
}