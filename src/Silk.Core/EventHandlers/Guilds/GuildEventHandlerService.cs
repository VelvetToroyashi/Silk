using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.MediatR.Users;
using Silk.Core.Data.Models;
using Silk.Core.Utilities;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;
using Silk.Shared.Constants;

namespace Silk.Core.EventHandlers.Guilds
{
    public sealed class GuildEventHandlerService : BackgroundService
    {

        private const string OnGuildJoinThankYouMessage = "Hiya! My name is Silk! I hope to satisfy your entertainment and moderation needs." +
                                                          "\n\nI respond to mentions and `s!` by default, but you can change that with `s!prefix`" +
                                                          "\n\nThere's also a variety of :sparkles: slash commands :sparkles: if those suit your fancy!" +
                                                          "\n\nAlso! Development, hosting, infrastructure, etc. is expensive! " +
                                                          "\nDonations via Ko-Fi *greatly* aid in this endevour. <3";
        private readonly DiscordShardedClient _client;
        private readonly ILogger<GuildEventHandlerService> _logger;

        private readonly IMediator _mediator;
        private bool _cachedAllInitialGuilds;
        private int _currentShardsCompleted; // How many shards have fired GUILD_DOWNLOAD_COMPLETE. //

        private Dictionary<int, int> _guilds;

        private bool _logged;

        private int _shardCount; // How many shards to wait for. //
        private DateTime? _startTime;
        public GuildEventHandlerService(IMediator mediator, DiscordShardedClient client, ILogger<GuildEventHandlerService> logger)
        {
            _mediator = mediator;
            _client = client;
            _logger = logger;
        }
        public ConcurrentQueue<Func<Task>> CacheQueue { get; } = new();

        internal void MarkCompleted(int shardId)
        {
            _currentShardsCompleted++;
        }


        internal async Task CacheGuildAsync(DiscordGuild guild, int shardId)
        {
            _startTime ??= DateTime.Now;
            await _mediator.Send(new GetOrCreateGuildRequest(guild.Id, StringConstants.DefaultCommandPrefix));

            int members = await CacheMembersAsync(guild.Members.Values);
            ++_guilds[shardId];

            LogMembers(members, guild.Members.Count, shardId);
            CheckForCompletion();
        }

        internal async Task JoinedGuild(GuildCreateEventArgs args)
        {
            _logger.LogInformation("Joined new guild! {GuildName} | {GuildMemberCount} members", args.Guild.Name, args.Guild.MemberCount);

            var bot = args.Guild.CurrentMember;
            
            var thankYouChannel = args.Guild
                .Channels.Values
                .OrderBy(c => c.Position)
                .FirstOrDefault(c => c.Type is ChannelType.Text && c.PermissionsFor(bot).HasPermission(Permissions.SendMessages | Permissions.EmbedLinks));
            
            if (thankYouChannel is null)
                return; // All channels are locked. //

            DiscordEmbedBuilder? embed = new DiscordEmbedBuilder()
                .WithTitle("Thank you for adding me!")
                .WithColor(new("94f8ff"))
                .WithDescription(OnGuildJoinThankYouMessage)
                .WithThumbnail("https://files.velvetthepanda.dev/silk.png")
                .WithFooter("Silk! | Made by Velvet & Contributors w/ <3");

            DiscordMessageBuilder? builder = new DiscordMessageBuilder()
                .WithEmbed(embed)
                .AddComponents(new DiscordLinkButtonComponent("https://ko-fi.com/velvetthepanda", "Ko-Fi!"),
                    new DiscordLinkButtonComponent("https://discord.gg/HZfZb95", "Support server!"),
                    new DiscordLinkButtonComponent($"https://discord.com/api/oauth2/authorize?client_id={_client.CurrentApplication.Id}&permissions=502656214&scope=bot%20applications.commands", "Invite me!"),
                    new DiscordLinkButtonComponent("https://github.com/VelvetThePanda/Silk", "Source code!"));

            await thankYouChannel.SendMessageAsync(builder);
            await CacheGuildAsync(args.Guild, args.Guild.GetClient().ShardId);
        }

        private void LogMembers(int members, int totalMembers, int shardId)
        {
            string message;
            if (members is 0)
            {
                message = "Guild cached! Shard [{shard}/{shards}] → Guild [{currentGuild}/{guilds}]";
                _logger.LogDebug(message, shardId + 1,
                    _client.ShardClients.Count,
                    _guilds[shardId], _client.ShardClients[shardId].Guilds.Count);
            }
            else
            {
                message = "Guild cached! Shard [{shard}/{shards}] → Guild [{currentGuild}/{guilds}] → Staff [{members}/{allMembers}]";
                _logger.LogDebug(message, shardId + 1,
                    _client.ShardClients.Count,
                    _guilds[shardId], _client.ShardClients[shardId].Guilds.Count,
                    members, totalMembers);
            }
        }

        private void CheckForCompletion()
        {
            int totalShards = _client.ShardClients.Count;
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
            }
        }

        private async Task<int> CacheMembersAsync(IEnumerable<DiscordMember> members)
        {
            var staffCount = 0;
            List<DiscordMember> staff = members.Where(m => !m.IsBot && m.Permissions.HasPermission(FlagConstants.CacheFlag)).ToList();

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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield(); // Sync until an await, which means we block until we finish the queue. //
            _shardCount = _client.ShardClients.Count;
            _guilds = new(_shardCount);

            for (var i = 0; i < _shardCount; i++)
                _guilds.Add(i, 0);

            while (!stoppingToken.IsCancellationRequested)
            {
                while (!_cachedAllInitialGuilds && _currentShardsCompleted != _shardCount)
                    await Task.Delay(200, stoppingToken);

                if (!CacheQueue.IsEmpty)
                {
                    lock (CacheQueue)
                    {
                        foreach (var t in CacheQueue)
                            AsyncUtil.RunSync(t); 
                        CacheQueue.Clear();
                    }
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}