using JetBrains.Annotations;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;
using Remora.Results;

namespace Silk.Interactivity;

[PublicAPI]
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


    public Task<Result<IInteractionCreate?>> WaitForButtonAsync(IUser user, IMessage message, CancellationToken ct = default)
        => WaitForButtonAsync(ev => GetUser(ev).IsDefined(out var evUser)   && evUser.ID    == user.ID &&
                                    ev.Message.IsDefined(out var evMessage) && evMessage.ID == message.ID,
                              ct);

    public Task<Result<IInteractionCreate?>> WaitForButtonAsync(Func<IInteractionCreate, bool> predicate, CancellationToken ct = default)
        => _interactionWaiter.WaitForEventAsync(predicate, ct);

    public Task<Result<IMessageCreate?>> WaitForMessageAsync(IPartialUser user, CancellationToken ct = default)
        => _messageWaiter.WaitForEventAsync(ev => ev.Author.ID == user.ID, ct);

    public Task<Result<IMessageCreate?>> WaitForMessageAsync(Func<IMessage, bool> predicate, CancellationToken ct = default)
        => _messageWaiter.WaitForEventAsync(predicate, ct);
    
    
    private Optional<IUser> GetUser(IInteraction interaction) 
        => interaction.Member.IsDefined(out var member) ? member.User : interaction.User;
}