using DSharpPlus;
using DSharpPlus.EventArgs;
using MediatR;

namespace Silk.Core.Discord.EventHandlers.Notifications
{
    public record GuildCreated(DiscordClient Client, GuildCreateEventArgs Args) : INotification;
}