using DSharpPlus;
using DSharpPlus.EventArgs;
using MediatR;

namespace Silk.Core.EventHandlers.Notifications
{
    public record GuildAvailable(DiscordClient Client, GuildCreateEventArgs Args) : INotification;
}