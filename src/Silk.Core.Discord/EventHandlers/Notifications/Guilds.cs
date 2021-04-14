using DSharpPlus;
using DSharpPlus.EventArgs;
using MediatR;

namespace Silk.Core.Discord.EventHandlers.Notifications
{
    public record GuildCreated(GuildCreateEventArgs Args, DiscordClient Client) : INotification;
}