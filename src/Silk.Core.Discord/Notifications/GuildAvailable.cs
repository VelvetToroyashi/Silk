using DSharpPlus;
using DSharpPlus.EventArgs;
using MediatR;

namespace Silk.Core.Discord.EventHandlers.Notifications
{
    public record GuildAvailable(DiscordClient Client, GuildCreateEventArgs Args) : INotification;
}