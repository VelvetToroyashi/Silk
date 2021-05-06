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
using Silk.Core.Discord.Types;
using Silk.Extensions;
using Silk.Shared.Constants;

namespace Silk.Core.Discord.EventHandlers.Guilds
{
    public class GuildEventHandlerService : IHostedService
    {
        public ConcurrentQueue<Lazy<Task>> CacheQueue { get; } = new();

        private bool _logged;
        private DateTime? _startTime;
        private bool _cachedAllInitialGuilds;


        private readonly Main _bot;
        private readonly IMediator _mediator;
        private readonly DiscordShardedClient _client;
        private readonly ILogger<GuildEventHandlerService> _logger;

        private Dictionary<int, int> _guilds;

        private int _shardCount; // How many shards to wait for. //
        private int _currentShardsCompleted; // How many shards have fired GUILD_DOWNLOAD_COMPLETE. //
        public GuildEventHandlerService(IMediator mediator, DiscordShardedClient client, ILogger<GuildEventHandlerService> logger, Main bot)
        {
            _mediator = mediator;
            _client = client;
            _logger = logger;
            _bot = bot;
        }

        private const string OnGuildJoinThankYouMessage = "Hiya! My name is Silk! I hope to satisfy your entertainment and moderation needs. I respond to mentions and `s!` by default, but you can change the prefix by using the prefix command.\n" +
                                                          "Also! Development, hosting, infrastructure, etc. is expensive! Donations via [Patreon](https://patreon.com/VelvetThePanda) and [Ko-Fi](https://ko-fi.com/velvetthepanda) *greatly* aid in this endevour. <3";


        public async Task StartAsync(CancellationToken cancellationToken) => ExecuteAsync(cancellationToken); // This will stop itself. IHostedService blocks other services while its starting. //
        public async Task StopAsync(CancellationToken cancellationToken) { }

        internal void MarkCompleted(int shardId) => _currentShardsCompleted++;


        internal async Task CacheGuildAsync(DiscordGuild guild, int shardId)
        {
            _startTime ??= DateTime.Now;
            _bot.ChangeState(BotState.Caching);
            await _mediator.Send(new GetOrCreateGuildRequest(guild.Id, Main.DefaultCommandPrefix));

            int members = await CacheMembersAsync(guild.Members.Values);
            ++_guilds[shardId];

            LogMembers(members, guild.Members.Count, shardId);
            CheckForCompletion();
        }

        internal async Task JoinedGuild(GuildCreated guildNotification)
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

        private void LogMembers(int members, int totalMembers, int shardId)
        {
            string message;
            if (members is 0)
            {
                message = "Caching Complete! Shard [{shard}/{shards}] → Guild [{currentGuild}/{guilds}]";
                _logger.LogDebug(message, shardId + 1,
                    _bot.ShardClient.ShardClients.Count,
                    _guilds[shardId], _client.ShardClients[shardId].Guilds.Count);
            }
            else
            {
                message = "Caching Complete! Shard [{shard}/{shards}] → Guild [{currentGuild}/{guilds}] → Staff [{members}/{allMembers}]";
                _logger.LogDebug(message, shardId + 1,
                    _bot.ShardClient.ShardClients.Count,
                    _guilds[shardId], _client.ShardClients[shardId].Guilds.Count,
                    members, totalMembers);
            }
        }

        private void CheckForCompletion()
        {
            var totalShards = _client.ShardClients.Count;
            var completedShards = new bool[totalShards];

            for (var i = 0; i < totalShards; i++)
                completedShards[i] = _guilds[i] == _client.ShardClients[i].Guilds.Count;

            _cachedAllInitialGuilds = completedShards.All(b => b);

            if (_cachedAllInitialGuilds && !_logged)
            {
                _logger.LogInformation("Finished caching {Guilds} guilds for {Shards} shards in {Time:N1} ms!",
                    _client.ShardClients.Values.SelectMany(s => s.Guilds).Count(),
                    _client.ShardClients.Count, (DateTime.Now - _startTime).Value.TotalMilliseconds);
                _logged = true;
                _bot.ChangeState(BotState.Ready);
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
                    {
                        staffCount++;
                        user.Flags.Add(flag);
                    }
                    await _mediator.Send(new UpdateUserRequest(member.Guild.Id, member.Id, user.Flags));
                }
                else
                {
                    await _mediator.Send(new AddUserRequest(member.Guild.Id, member.Id, flag));
                    staffCount++;
                }
            }
            return Math.Max(staffCount, 0);
        }

        private async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _shardCount = _bot.ShardClient.ShardClients.Count;
            _guilds = new(_shardCount);

            for (var i = 0; i < _shardCount; i++)
                _guilds.Add(i, 0);

            while (!stoppingToken.IsCancellationRequested)
            {
                while (!_cachedAllInitialGuilds && _currentShardsCompleted != _shardCount)
                    await Task.Delay(200, stoppingToken);

                if (!CacheQueue.IsEmpty)
                    foreach (var t in CacheQueue)
                        await t.Value;

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}