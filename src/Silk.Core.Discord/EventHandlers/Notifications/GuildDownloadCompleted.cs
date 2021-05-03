using DSharpPlus;
using DSharpPlus.EventArgs;
using MediatR;

namespace Silk.Core.Discord.EventHandlers.Notifications
{
    public record GuildDownloadCompleted(DiscordClient Client, GuildDownloadCompletedEventArgs Args) : INotification;
}