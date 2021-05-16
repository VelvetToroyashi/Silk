using DSharpPlus;
using DSharpPlus.EventArgs;
using MediatR;

namespace Silk.Core.EventHandlers.Notifications
{
    public record GuildCreated(DiscordClient Client, GuildCreateEventArgs Args) : INotification;
}