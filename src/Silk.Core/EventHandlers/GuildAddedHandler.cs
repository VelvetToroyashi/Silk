using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MediatR;
using Microsoft.Extensions.Logging;
using Silk.Core.Constants;
using Silk.Data.MediatR;
using Silk.Data.Models;
using Silk.Extensions;

namespace Silk.Core.EventHandlers
{
    //This relies on multiple events to update its state, so we can't implement INotificationHandler.
    public class GuildAddedHandler
    {
        public static bool StartupCompleted { get; private set; }

        private readonly IMediator _mediator;
        private readonly ILogger<GuildAddedHandler> _logger;
        private readonly Dictionary<int, ShardState> _shardStates = new();
        private readonly object _lock = new();

        private struct ShardState
        {
            public bool Completed { get; set; }
            public int CachedGuilds { get; set; }
            public int CachedMembers { get; set; }
        }

        public GuildAddedHandler(ILogger<GuildAddedHandler> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
            IReadOnlyDictionary<int, DiscordClient> shards = Bot.Instance!.Client.ShardClients;
            if (shards.Count is 0)
                throw new ArgumentOutOfRangeException(nameof(DiscordClient.ShardCount), "Shards must be greater than 0");

            foreach ((int key, _) in shards)
                _shardStates.Add(key, new());
        }


        /// <summary>
        /// Caches and logs members when GUILD_AVAILABLE is fired via the gateway.
        /// </summary>
        public async Task OnGuildAvailable(DiscordClient client, GuildCreateEventArgs eventArgs)
        {
            Guild guild = await _mediator.Send(new GuildRequest.GetOrCreate(eventArgs.Guild.Id, Bot.DefaultCommandPrefix));
        int cachedMembers = await CacheGuildMembers(eventArgs.Guild.Members.Values);
            
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
        public async Task SendThankYouMessage(DiscordClient c, GuildCreateEventArgs e)
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

        private async Task<int> CacheGuildMembers(IEnumerable<DiscordMember> members)
        {
            int staffCount = 0;
            IEnumerable<DiscordMember> staff = members.Where(m => !m.IsBot);

            foreach (var member in staff)
            {
                UserFlag flag = member.HasPermission(Permissions.Administrator) || member.IsOwner ? UserFlag.EscalatedStaff : UserFlag.Staff;

                User? user = await _mediator.Send(new UserRequest.Get(member.Guild.Id, member.Id));
                if (user is not null)
                {
                    if (member.HasPermission(Permissions.Administrator) || member.IsOwner && !user.Flags.Has(UserFlag.EscalatedStaff))
                    {
                        user.Flags.Add(UserFlag.EscalatedStaff);
                    }
                    else if (member.HasPermission(PermissionConstants.CacheFlag))
                    {
                        user.Flags.Add(UserFlag.Staff);
                    }
                    else
                    {
                        if (user.Flags.Has(UserFlag.Staff))
                        {
                            UserFlag f = user.Flags.Has(UserFlag.EscalatedStaff) ? UserFlag.EscalatedStaff : UserFlag.Staff;
                            user.Flags.Remove(f);
                        }
                    }
                    await _mediator.Send(new UserRequest.Update(member.Guild.Id, member.Id) {Flags = user.Flags});
                }
                else if (member.HasPermission(PermissionConstants.CacheFlag) || member.IsAdministrator() || member.IsOwner)
                {
                    await _mediator.Send(new UserRequest.Add(member.Guild.Id, member.Id, flag));
                    staffCount++;
                }
            }
            return Math.Max(staffCount, 0);
        }
    }
}