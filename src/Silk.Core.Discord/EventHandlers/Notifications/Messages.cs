using DSharpPlus;
using DSharpPlus.EventArgs;
using MediatR;
using Silk.Shared.Abstractions.DSharpPlus.Concrete;

namespace Silk.Core.Discord.EventHandlers.Notifications
{
    public record MessageCreated(DiscordClient Client, Message Message) : INotification;

    public record MessageEdited(DiscordClient Client, MessageUpdateEventArgs EventArgs) : INotification;

    public record MessageDeleted(DiscordClient Client, MessageDeleteEventArgs EventArgs) : INotification;
}