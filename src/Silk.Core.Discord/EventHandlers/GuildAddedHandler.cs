using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dasync.Collections;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MediatR;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.MediatR.Users;
using Silk.Core.Data.Models;
using Silk.Core.Discord.Types;
using Silk.Extensions;
using Silk.Shared.Constants;

namespace Silk.Core.Discord.EventHandlers
{
    //This relies on multiple events to update its state, so we can't implement INotificationHandler.
    // Correction. I'm stupid. You can implement multiple interfaces. Don't listen to the above comment. ~Velvet.//
    public class GuildAddedHandler
    {
        private bool _logged;
        private bool _startupCompleted;

        private readonly IMediator _mediator;
        private readonly object _lock = new();
        private readonly ILogger<GuildAddedHandler> _logger;
        private readonly Dictionary<int, ShardState> _shardStates = new();



        private const string BotJoinGreetingMessage = "Hiya! My name is Silk! I hope to satisfy your entertainment and moderation needs. I respond to mentions and `s!` by default, but you can change the prefix by using the prefix command.\n" +
                                                      "Also! Development, hosting, infrastructure, etc. is expensive! Donations via [Patreon](https://patreon.com/VelvetThePanda) and [Ko-Fi](https://ko-fi.com/velvetthepanda) *greatly* aid in this endevour. <3";

        private class ShardState
        {
            public bool Completed { get; set; }
            public int CachedGuilds { get; set; }
            public int CachedMembers { get; set; }
        }

        public GuildAddedHandler(ILogger<GuildAddedHandler> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
            IReadOnlyDictionary<int, DiscordClient> shards = Main.ShardClient.ShardClients;

            foreach (var key in shards.Keys)
                _shardStates.Add(key, new());
        }

        /// <summary>
        ///     Caches and logs members when GUILD_AVAILABLE is fired via the gateway.
        /// </summary>
        public async Task OnGuildAvailable(DiscordClient client, GuildCreateEventArgs eventArgs)
        {
            if (_startupCompleted) return;

            Main.ChangeState(BotState.Caching);

            await _mediator.Send(new GetOrCreateGuildRequest(eventArgs.Guild.Id, Main.DefaultCommandPrefix));
            int cachedMembers = await CacheGuildMembers(eventArgs.Guild.Members.Values);

            lock (_lock)
            {
                ShardState state = _shardStates[client.ShardId];
                state.CachedMembers += cachedMembers;
                ++state.CachedGuilds;
                _shardStates[client.ShardId] = state;

                if (!_startupCompleted)
                    LogCachedMemberCount(client, eventArgs, cachedMembers, state);
            }
        }


        public async Task OnGuildDownloadComplete(DiscordClient c, GuildDownloadCompletedEventArgs e)
        {
            _shardStates[c.ShardId].Completed = true;
            _startupCompleted = _shardStates.Values.All(s => s.Completed);

            if (_startupCompleted && !_logged)
            {
                _logger.LogDebug("Cache runs complete! Runtime: {Runtime}", 0);
                _logged = true;
                Main.ChangeState(BotState.Ready);
            }
        }


        // Used in conjunction with OnGuildJoin() //
        public async Task SendThankYouMessage(DiscordClient c, GuildCreateEventArgs e)
        {
            var allChannels = (await e.Guild.GetChannelsAsync()).OrderBy(channel => channel.Position);
            DiscordMember bot = e.Guild.CurrentMember;
            DiscordChannel? availableChannel =
                allChannels.Where(c => c.Type is ChannelType.Text)
                    .FirstOrDefault(c => c.PermissionsFor(bot).HasPermission(Permissions.SendMessages | Permissions.EmbedLinks));

            if (availableChannel is null) return; // Server prohibits talking in all channels. No point in waiting. //

            var builder = new DiscordEmbedBuilder()
                .WithTitle("Thank you for adding me!")
                .WithColor(new("94f8ff"))
                .WithDescription(BotJoinGreetingMessage)
                .WithThumbnail("https://files.velvetthepanda.dev/silk.png")
                .WithFooter("Silk! | Made with <3 by the Velvet & Contributors");
            await availableChannel.SendMessageAsync(builder);
        }

        private async Task<int> CacheGuildMembers(IEnumerable<DiscordMember> members)
        {
            int staffCount = 0;
            List<DiscordMember> staff = members.Where(m => !m.IsBot).ToList();

            if (staff.Count > 500)
            {
                var asyncStaff = staff.ToAsyncEnumerable();
                await asyncStaff.ParallelForEachAsync(CacheMemberasync, 4);
            }
            else
            {
                foreach (var member in staff)
                    await CacheMemberasync(member);
            }

            return Math.Max(staffCount, 0);

            async Task CacheMemberasync(DiscordMember member)
            {

                UserFlag flag = member.HasPermission(Permissions.Administrator) || member.IsOwner ? UserFlag.EscalatedStaff : UserFlag.Staff;

                User? user = await _mediator.Send(new GetUserRequest(member.Guild.Id, member.Id));

                if (user is not null)
                {
                    if (member.HasPermission(FlagConstants.CacheFlag))
                    {
                        user.Flags.Add(UserFlag.Staff);
                    }
                    else if (user.Flags.Has(flag))
                    {
                        user.Flags.Remove(flag);
                    }

                    await _mediator.Send(new UpdateUserRequest(member.Guild.Id, member.Id, user.Flags));
                }
                else if (member.HasPermission(FlagConstants.CacheFlag) || member.IsAdministrator() || member.IsOwner)
                {
                    await _mediator.Send(new AddUserRequest(member.Guild.Id, member.Id, flag));
                    staffCount++;
                }
            }
        }

        private void LogCachedMemberCount(DiscordClient client, GuildCreateEventArgs eventArgs, int cachedMembers, ShardState state)
        {
            string message;
            if (cachedMembers is 0)
            {
                message = "Cached Guild! Shard [{shard}/{shards}] → Guild [{currentGuild}/{guilds}] → Staff [No new staff!]";
                _logger.LogDebug(message, client.ShardId + 1,
                    Bot.Instance!.Client.ShardClients.Count,
                    state.CachedGuilds, client.Guilds.Count);
            }
            else
            {
                message = "Cached Guild! Shard [{shard}/{shards}] → Guild [{currentGuild}/{guilds}] → Staff [{members}/{allMembers}]";
                _logger.LogDebug(message, client.ShardId + 1,
                    Bot.Instance!.Client.ShardClients.Count,
                    state.CachedGuilds, client.Guilds.Count,
                    cachedMembers, eventArgs.Guild.Members.Count);
            }
        }
    }
}