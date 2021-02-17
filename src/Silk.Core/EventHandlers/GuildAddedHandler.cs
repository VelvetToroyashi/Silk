using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Silk.Core.Constants;
using Silk.Core.Database.Models;
using Silk.Core.Services.Interfaces;
using Silk.Extensions;

namespace Silk.Core.EventHandlers
{
    public class GuildAddedHandler
    {
        public static bool StartupCompleted { get; private set; }

        private readonly IDatabaseService _dbService;
        private readonly ILogger<GuildAddedHandler> _logger;
        private readonly Dictionary<int, ShardState> _shardStates = new();
        private readonly object _lock = new();

        private struct ShardState
        {
            public bool Completed { get; set; }
            public int CachedGuilds { get; set; }
            public int CachedMembers { get; set; }
        }

        public GuildAddedHandler(ILogger<GuildAddedHandler> logger, IDatabaseService dbService)
        {
            _logger = logger;
            _dbService = dbService;
            IReadOnlyDictionary<int, DiscordClient> shards = Bot.Instance!.Client.ShardClients;
            if (shards.Count is 0)
            {
                _logger.LogCritical("Shard count is 0. Cache running requires at least 1 shard!");
                throw new ArgumentOutOfRangeException(nameof(DiscordClient.ShardCount), "Shards must be > 0");
            }

            foreach ((int key, _) in shards)
                _shardStates.Add(key, new());
        }


        /// <summary>
        /// Caches and logs members when GUILD_AVAILABLE is fired via the gateway.
        /// </summary>
        public async Task OnGuildAvailable(DiscordClient client, GuildCreateEventArgs eventArgs)
        {
            Guild guild = await _dbService.GetOrCreateGuildAsync(eventArgs.Guild.Id);
            int cachedMembers = CacheGuildMembers(guild, eventArgs.Guild.Members.Values);
            await _dbService.UpdateGuildAsync(guild);
            // Create a state object and update values. 

            lock (_lock)
            {
                ShardState state = _shardStates[client.ShardId];
                state.CachedMembers += cachedMembers;
                ++state.CachedGuilds;
                _shardStates[client.ShardId] = state;
                if (!StartupCompleted)
                {
                    string message = $"Cached Guild! Shard [{client.ShardId + 1}/{Bot.Instance!.Client.ShardClients.Count}] → Guild [{state.CachedGuilds}/{client.Guilds.Count}]";
                    message += cachedMembers is 0 ?
                        " → Staff [No new staff!]" :
                        $" → Staff [{cachedMembers}/{eventArgs.Guild.Members.Count}]";

                    _logger.LogDebug(message);
                }
            }
        }

        public async Task OnGuildDownloadComplete(DiscordClient c, GuildDownloadCompletedEventArgs e)
        {
            ShardState state = _shardStates[c.ShardId];
            state.Completed = true;
            _shardStates[c.ShardId] = state;
            StartupCompleted = _shardStates.Values.All(s => s.Completed);
            if (StartupCompleted) _logger.LogDebug("All shard(s) cache runs complete!");
        }


        // Used in conjunction with OnGuildJoin() //
        public async Task SendWelcomeMessage(DiscordClient c, GuildCreateEventArgs e)
        {
            var allChannels = (await e.Guild.GetChannelsAsync()).OrderBy(channel => channel.Position);
            DiscordMember bot = e.Guild.CurrentMember;
            DiscordChannel? availableChannel =
                allChannels.Where(c => c.Type is ChannelType.Text)
                    .FirstOrDefault(c => c.PermissionsFor(bot).HasPermission(Permissions.SendMessages | Permissions.EmbedLinks));
            if (availableChannel is null) return;


            var builder = new DiscordEmbedBuilder()
                .WithTitle("Thank you for adding me!")
                .WithColor(new("94f8ff"))
                .WithThumbnail("https://files.velvetthepanda.dev/silk.png")
                .WithFooter("Did I break? DM me ticket create [message] and I'll forward it to the owners <3");
            await availableChannel.SendMessageAsync(builder);
        }
        private static int CacheGuildMembers(Guild guild, IEnumerable<DiscordMember> members)
        {
            int staffBefore = guild.Users.Count(u => u.Flags.HasFlag(UserFlag.Staff));
            IEnumerable<DiscordMember> staff = members.Where(m => !m.IsBot);

            foreach (var member in staff)
            {
                UserFlag flag = member.HasPermission(Permissions.Administrator) || member.IsOwner ? UserFlag.EscalatedStaff : UserFlag.Staff;

                if (guild.Users.FirstOrDefault(u => u.Id == member.Id) is var user and not null)
                {
                    user.Flags =
                        user.Flags.Has(flag) ?
                            user.Flags.Remove(flag) :
                            user.Flags.Add(flag);
                }
                else if (member.HasPermission(PermissionConstants.CacheFlag) || member.IsAdministrator() || member.IsOwner)
                {
                    guild.Users.Add(new() {Id = member.Id, Flags = flag});
                }
            }
            int staffNow = guild.Users.Count(us => us.Flags.HasFlag(UserFlag.Staff));
            int staffCount = staffNow - staffBefore;
            return staffCount < 1 ? 0 : staffCount;
        }
    }
}