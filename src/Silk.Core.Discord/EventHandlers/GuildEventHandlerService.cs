using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.MediatR.Users;
using Silk.Core.Data.Models;
using Silk.Core.Discord.EventHandlers.Notifications;
using Silk.Extensions;
using Silk.Shared.Constants;

namespace Silk.Core.Discord.EventHandlers
{
    public class GuildEventHandlerService : IHostedService, INotificationHandler<GuildCreated>, INotificationHandler<GuildAvailable>, INotificationHandler<GuildDownloadCompleted>
    {
        private bool _cachedAllInitialGuilds;

        private readonly IMediator _mediator;
        private readonly DiscordShardedClient _client;
        private readonly ILogger<GuildEventHandlerService> _logger;

        private readonly ConcurrentQueue<Task> _cacheQueue = new();
        private readonly Dictionary<int, int> _guilds = new();

        private readonly int _shardCount; // How many shards to wait for. //
        private int _currentShardsCompleted; // How many shards have fired GUILD_DOWNLOAD_COMPLETE. //
        public GuildEventHandlerService(IMediator mediator, DiscordShardedClient client, ILogger<GuildEventHandlerService> logger)
        {
            _mediator = mediator;
            _client = client;
            _logger = logger;

            _shardCount = _client.ShardClients.Count;
            _currentShardsCompleted = 0;

            Console.WriteLine("Poggers.");
        }

        private const string OnGuildJoinThankYouMessage = "Hiya! My name is Silk! I hope to satisfy your entertainment and moderation needs. I respond to mentions and `s!` by default, but you can change the prefix by using the prefix command.\n" +
                                                          "Also! Development, hosting, infrastructure, etc. is expensive! Donations via [Patreon](https://patreon.com/VelvetThePanda) and [Ko-Fi](https://ko-fi.com/velvetthepanda) *greatly* aid in this endevour. <3";


        private async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var shardCount = _client.ShardClients.Count;

            while (stoppingToken.IsCancellationRequested)
            {
                if (_cachedAllInitialGuilds)
                {
                    await Task.Delay(10000, stoppingToken);
                    continue;
                }

                while (_currentShardsCompleted != shardCount)
                    await Task.Delay(200, stoppingToken);

                foreach (var t in _cacheQueue)
                    await t;

                await Task.Delay(-1, stoppingToken);
            }
        }

        private async Task CacheGuildAsync(DiscordGuild guild, int shardId)
        {
            await _mediator.Send(new GetOrCreateGuildRequest(guild.Id, Main.DefaultCommandPrefix));
            int members = await CacheMembersAsync(guild.Members.Values);
            _guilds[shardId]++;
            LogMembers(members, guild.Members.Count, shardId);
        }

        private void LogMembers(int members, int totalMembers, int shardId)
        {
            string message;
            if (members is 0)
            {
                message = "Cached Guild! Shard [{shard}/{shards}] → Guild [{currentGuild}/{guilds}] → Staff [No new staff!]";
                _logger.LogDebug(message, shardId + 1,
                    Main.ShardClient.ShardClients.Count,
                    _guilds[shardId], _client.ShardClients[shardId].Guilds.Count);
            }
            else
            {
                message = "Cached Guild! Shard [{shard}/{shards}] → Guild [{currentGuild}/{guilds}] → Staff [{members}/{allMembers}]";
                _logger.LogDebug(message, shardId + 1,
                    Main.ShardClient.ShardClients.Count,
                    _guilds[shardId], _client.ShardClients[shardId].Guilds.Count,
                    members, totalMembers);
            }
        }

        private async Task<int> CacheMembersAsync(IEnumerable<DiscordMember> members)
        {
            int staffCount = 0;
            List<DiscordMember> staff = members.Where(m => !m.IsBot && (m.HasPermission(FlagConstants.CacheFlag) || m.IsAdministrator() || m.IsOwner)).ToList();

            foreach (var member in staff)
            {
                UserFlag flag = member.HasPermission(Permissions.Administrator) || member.IsOwner ? UserFlag.EscalatedStaff : UserFlag.Staff;

                User? user = await _mediator.Send(new GetUserRequest(member.Guild.Id, member.Id));
                if (user is not null)
                {
                    if (!user.Flags.Has(flag))
                        user.Flags.Add(flag);

                    await _mediator.Send(new UpdateUserRequest(member.Guild.Id, member.Id, user.Flags));
                    staffCount++;
                }
                else
                {
                    await _mediator.Send(new AddUserRequest(member.Guild.Id, member.Id, flag));
                    staffCount++;
                }
            }
            return Math.Max(staffCount, 0);
        }

        private async Task JoinedGuild(GuildCreated guildNotification)
        {
            var guildEvent = guildNotification.Args;

            var allChannels = (await guildEvent.Guild.GetChannelsAsync()).OrderBy(channel => channel.Position);
            DiscordMember bot = guildEvent.Guild.CurrentMember;
            DiscordChannel? availableChannel = allChannels
                .Where(c => c.Type is ChannelType.Text)
                .FirstOrDefault(c => c.PermissionsFor(bot).HasPermission(Permissions.SendMessages | Permissions.EmbedLinks));

            if (availableChannel is null)
                return;

            var builder = new DiscordEmbedBuilder()
                .WithTitle("Thank you for adding me!")
                .WithColor(new("94f8ff"))
                .WithDescription(OnGuildJoinThankYouMessage)
                .WithThumbnail("https://files.velvetthepanda.dev/silk.png")
                .WithFooter("Silk! | Made by Velvet & Contributors w/ <3");
            await availableChannel.SendMessageAsync(builder);
        }

        public async Task Handle(GuildCreated notification, CancellationToken cancellationToken) => _cacheQueue.Enqueue(JoinedGuild(notification));
        public async Task Handle(GuildAvailable notification, CancellationToken cancellationToken) => _cacheQueue.Enqueue(CacheGuildAsync(notification.Args.Guild, notification.Client.ShardId));
        public async Task Handle(GuildDownloadCompleted notification, CancellationToken cancellationToken) => _currentShardsCompleted++;

        public async Task StartAsync(CancellationToken cancellationToken) => ExecuteAsync(cancellationToken);
        public async Task StopAsync(CancellationToken cancellationToken) { }
    }
}