using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace Silk.Core.EventHandlers.Guilds
{
    public class GuildEventHandlers
    {
        private readonly GuildEventHandlerService _guildHandler;
        public GuildEventHandlers(GuildEventHandlerService guildHandler) => _guildHandler = guildHandler;

        public async Task OnGuildJoin(DiscordClient client, GuildCreateEventArgs args) =>
            _guildHandler.CacheQueue.Enqueue(new(GuildEventHandlerService.JoinedGuild(args)));
        public async Task OnGuildAvailable(DiscordClient client, GuildCreateEventArgs args, CancellationToken cancellationToken) =>
            _guildHandler.CacheQueue.Enqueue(new(() => _guildHandler.CacheGuildAsync(args.Guild, client.ShardId)));
        public async Task OnGuildDownload(DiscordClient client, GuildDownloadCompletedEventArgs _) => _guildHandler.MarkCompleted(client.ShardId);
    }
}