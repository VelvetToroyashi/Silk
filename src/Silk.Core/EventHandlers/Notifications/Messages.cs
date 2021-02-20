using DSharpPlus;
using DSharpPlus.EventArgs;
using MediatR;

namespace Silk.Core.EventHandlers.Notifications
{
    public record MessageCreated(DiscordClient Client, MessageCreateEventArgs EventArgs) : INotification;

    public record MessageUpdated(DiscordClient Client, MessageUpdateEventArgs EventArgs) : INotification;

    public record MessageDeleted(DiscordClient Client, MessageDeleteEventArgs EventArgs) : INotification;
}