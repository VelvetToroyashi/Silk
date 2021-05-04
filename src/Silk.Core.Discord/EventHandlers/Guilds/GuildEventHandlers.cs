using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Silk.Core.Discord.EventHandlers.Notifications;

namespace Silk.Core.Discord.EventHandlers.Guilds
{
    public class GuildEventHandlers : INotificationHandler<GuildAvailable> //, INotificationHandler<GuildCreated>,  INotificationHandler<GuildDownloadCompleted>
    {
        private readonly GuildEventHandlerService _guildHandler;
        private readonly ILogger<GuildEventHandlers> _logger;

        public GuildEventHandlers(GuildEventHandlerService guildHandler, ILogger<GuildEventHandlers> logger)
        {
            _guildHandler = guildHandler;
            _logger = logger;
            _logger.LogTrace("MediatR handler constructed");
        }

        public async Task Handle(GuildCreated notification, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Guild joined!");
            _guildHandler.CacheQueue.Enqueue(_guildHandler.JoinedGuild(notification));
            _logger.LogTrace("Enqued guild!");
        }
        public async Task Handle(GuildAvailable notification, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Guild available!");
            _guildHandler.CacheQueue.Enqueue(_guildHandler.CacheGuildAsync(notification.Args.Guild, notification.Client.ShardId));
            _logger.LogTrace("Enqued guild task!");
        }
        public async Task Handle(GuildDownloadCompleted notification, CancellationToken cancellationToken) => _guildHandler.MarkCompleted(notification.Client.ShardId);
    }
}