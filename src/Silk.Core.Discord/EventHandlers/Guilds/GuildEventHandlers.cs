using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Silk.Core.Discord.EventHandlers.Notifications;

namespace Silk.Core.Discord.EventHandlers.Guilds
{
    public class GuildEventHandlers : INotificationHandler<GuildAvailable>, INotificationHandler<GuildCreated>, INotificationHandler<GuildDownloadCompleted>
    {
        private readonly GuildEventHandlerService _guildHandler;
        private readonly ILogger<GuildEventHandlers> _logger;

        public GuildEventHandlers(GuildEventHandlerService guildHandler, ILogger<GuildEventHandlers> logger)
        {
            _guildHandler = guildHandler;
            _logger = logger;
        }

        public async Task Handle(GuildCreated notification, CancellationToken cancellationToken) =>
            _guildHandler.CacheQueue.Enqueue(new(_guildHandler.JoinedGuild(notification)));
        public async Task Handle(GuildAvailable notification, CancellationToken cancellationToken) =>
            _guildHandler.CacheQueue.Enqueue(new(() => _guildHandler.CacheGuildAsync(notification.Args.Guild, notification.Client.ShardId)));
        public async Task Handle(GuildDownloadCompleted notification, CancellationToken cancellationToken) => _guildHandler.MarkCompleted(notification.Client.ShardId);
    }
}