using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Discord.EventHandlers.Notifications;

namespace Silk.Core.Discord.EventHandlers.Guilds
{
    public class GuildEventHandlers : INotificationHandler<GuildCreated>, INotificationHandler<GuildAvailable>, INotificationHandler<GuildDownloadCompleted>
    {
        private readonly GuildEventHandlerService _guildHandler;

        public GuildEventHandlers(GuildEventHandlerService guildHandler) => _guildHandler = guildHandler;

        public async Task Handle(GuildCreated notification, CancellationToken cancellationToken) =>
            _guildHandler.CacheQueue.Enqueue(_guildHandler.JoinedGuild(notification));
        public async Task Handle(GuildAvailable notification, CancellationToken cancellationToken) =>
            _guildHandler.CacheQueue.Enqueue(_guildHandler.CacheGuildAsync(notification.Args.Guild, notification.Client.ShardId));
        public async Task Handle(GuildDownloadCompleted notification, CancellationToken cancellationToken) => _guildHandler.MarkCompleted(notification.Client.ShardId);
    }
}