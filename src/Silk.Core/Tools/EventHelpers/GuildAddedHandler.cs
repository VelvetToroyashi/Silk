using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Silk.Core.Database.Models;
using Silk.Core.Services.Interfaces;
using Silk.Extensions;

namespace Silk.Core.Tools.EventHelpers
{
    public class GuildAddedHandler
    {
        //private int currentGuild;
        private bool startupCacheCompleted;

        
        private readonly IDatabaseService _dbService;
        private readonly ILogger<GuildAddedHandler> _logger;
        
        private readonly Dictionary<int, int> _shards = new();
        private readonly Dictionary<int, bool> _completedShards = new(); 
        private readonly Dictionary<int, List<CacheObject>> _cacheQueue = new();
        private readonly Dictionary<int, (int, int)> _guildCounters = new();
        
        private record CacheObject(ulong Id, IEnumerable<DiscordMember> Members);
        
        public GuildAddedHandler(ILogger<GuildAddedHandler> logger, IDatabaseService dbService)
        {
            _logger = logger;
            _dbService = dbService;
            // Initialize the dictionary so we can have simple event handlers. //

            for (int i = 0; i < Bot.Instance!.Client.ShardClients.Count; i++)
            {
                _cacheQueue.TryAdd(i, new());
                _shards.TryAdd(i, 0);
                _completedShards.TryAdd(i, false);
                _guildCounters.TryAdd(i, new());
            }
        }

        // Run on startup to cache all members //
        public Task OnGuildAvailable(DiscordClient c, GuildCreateEventArgs e)
        {
            if (startupCacheCompleted) return Task.CompletedTask; // Don't re-cache. //

            int currentGuild = ++_shards[c.ShardId];
            
            _logger.LogTrace($"Adding cache object to queue: Shard [{c.ShardId + 1}/{Bot.Instance!.Client.ShardClients.Count}] | Guild [{currentGuild}/{c.Guilds.Count}]");
            _cacheQueue[c.ShardId].Add(new(e.Guild.Id, e.Guild.Members.Values));
            return Task.CompletedTask;
        }

        public Task OnGuildDownloadComplete(DiscordClient c, GuildDownloadCompletedEventArgs e)
        {
            _logger.LogTrace($"Guild download complete for shard {c.ShardId + 1}");
            _logger.LogTrace("Preparing to iterate over cache objects");
            
            bool[] completedShards = new bool[Bot.Instance!.Client.ShardClients.Count];

            for (int i = 0; i < Bot.Instance!.Client.ShardClients.Count; i++)
                completedShards[i] = _completedShards[i];
            startupCacheCompleted = completedShards.All(s => s);
            
            
            _ = Task.Run(async () =>
            {
                for (var i = 0; i < _cacheQueue[c.ShardId].Count; i++)
                {
                    await CacheGuildAsync(c.Guilds[_cacheQueue[c.ShardId][i].Id].Id);
                    await CacheMembersAsync(_cacheQueue[c.ShardId][i], c.ShardId);
                }
                _logger.LogInformation($"Iterated over {_guildCounters[c.ShardId].Item1} members | Collected {_guildCounters[c.ShardId].Item2} Staff members.");
            });
            return Task.CompletedTask;
        }

        // Run when Silk! joins a new guild. // 
        public async Task OnGuildJoin(DiscordClient c, GuildCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                await CacheGuildAsync(e.Guild.Id);
                await CacheMembersAsync(new(e.Guild.Id, e.Guild.Members.Values), c.ShardId);
                await SendWelcomeMessage(c, e);
            });
        }

        private async Task CacheMembersAsync(CacheObject guildT, int shardId)
        {
            Guild guild = await _dbService.GetOrCreateGuildAsync(guildT.Id);
            CacheMembersAsync(guild, guildT.Members, shardId);
            await _dbService.UpdateGuildAsync(guild);
        }

        private async Task CacheGuildAsync(ulong guild) => await _dbService.GetOrCreateGuildAsync(guild);

        // Used in conjunction with OnGuildJoin() //
        private async Task SendWelcomeMessage(DiscordClient c, GuildCreateEventArgs e)
        {
            IOrderedEnumerable<DiscordChannel> allChannels = (await e.Guild.GetChannelsAsync()).OrderBy(channel => channel.Position);
            DiscordMember botAsMember = e.Guild.CurrentMember;

            DiscordChannel firstChannel = allChannels.First(channel =>
                channel.PermissionsFor(botAsMember).HasPermission(Permissions.SendMessages) &&
                channel.Type == ChannelType.Text);

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle("Thank you for adding me!")
                .WithColor(new("94f8ff"))
                .WithThumbnail(c.CurrentUser.AvatarUrl)
                .WithFooter("Did I break? DM me ticket create [message] and I'll forward it to the owners <3");

            var sb = new StringBuilder();
            sb.Append("Thank you for choosing Silk! to join your server <3")
                .AppendLine("I am a relatively lightweight bot with many functions - partially in moderation, ")
                .AppendLine("partially in games, with many more features to come!")
                .Append("If there's an issue, feel free to [Open an issue on GitHub](https://github.com/VelvetThePanda/Silk/issues), ")
                .AppendLine("or if you're not familiar with GitHub, feel free")
                .AppendLine($"to message the developers directly via {Bot.DefaultCommandPrefix}`ticket create <your message>`.")
                .Append($"By default, the prefix is `{Bot.DefaultCommandPrefix}`, or <@{c.CurrentUser.Id}>, but this can be changed by {Bot.DefaultCommandPrefix}setprefix <your prefix here>.");

            embed.WithDescription(sb.ToString());

            await firstChannel.SendMessageAsync(embed);
        }
        private void CacheMembersAsync(Guild guild, IEnumerable<DiscordMember> members, int shardId)
        {
            IEnumerable<DiscordMember> staff = members.Where(m =>
                (m.HasPermission(Permissions.KickMembers | Permissions.ManageMessages)
                  || m.HasPermission(Permissions.Administrator)
                  || m.IsOwner) && !m.IsBot);
            IEnumerable<User> currentStaff = guild.Users.Where(u => u.Flags.Has(UserFlag.Staff));
            
            
            foreach (DiscordMember member in staff)
            {
                var flags = UserFlag.Staff;
                if (member.HasPermission(Permissions.Administrator) || member.IsOwner) flags.Add(UserFlag.EscalatedStaff);

                User? user = guild.Users.FirstOrDefault(u => u.Id == member.Id);
                if (user is not null) //If user exists
                {
                    if (!user.Flags.HasFlag(UserFlag.Staff)) // Has flag
                        user.Flags.Add(UserFlag.Staff); // Add flag
                    if (member.HasPermission(Permissions.Administrator) || member.IsOwner)
                        user.Flags.Add(UserFlag.EscalatedStaff);
                }
                else guild.Users.Add(new() {Id = member.Id, Flags = flags});
            }
            
            int totalMembers =  _guildCounters[shardId].Item1 + members.Count();
            int totalStaff = _guildCounters[shardId].Item2 + guild.Users.Except(currentStaff).Count();
            
            _guildCounters[shardId] = (totalMembers, totalStaff);

        }
    }
}