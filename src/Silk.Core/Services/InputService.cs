using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Silk.Core.Services.Interfaces;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Core.Services
{
    /// <inheritdoc cref="IInputService"/>
    public class InputService : IInputService
    {
        private readonly DiscordShardedClient _client;

        public InputService(DiscordShardedClient client)
        {
            _client = client;
        }

        public async Task<string?> GetStringInputAsync(ulong userId, ulong channelId, ulong? guildId = null, TimeSpan? timeOut = null)
        {
            var interactivity = GetInteractivityInternal(guildId);
            return await WaitForInputAsync(interactivity, userId, channelId, guildId, timeOut);
        }
        private InteractivityExtension GetInteractivityInternal(ulong? guildId)
        {
            if (guildId is null) { return _client.ShardClients[0].GetInteractivity(); } // DMs are handled on Shard 0 anyway. //
            else
            {
                DiscordClient guildShard = _client.ShardClients.Values.First(s => s.Guilds.Keys.Contains(guildId.Value));
                return guildShard.GetInteractivity();
            }
        }
        private async Task<string?> WaitForInputAsync(InteractivityExtension interactivity, ulong userId, ulong channelId, ulong? guildId, TimeSpan? timeOut)
        {
            var message = await interactivity.WaitForMessageAsync(m =>
            {
                var isUser = m.Author.Id == userId;
                var isChannel = m.ChannelId == channelId;
                var isGuild = m.Channel.GuildId == guildId;

                if (guildId is null)
                    return isUser && isChannel;
                else return isUser && isChannel && isGuild;
            }, timeOut);

            return message.TimedOut ? null : message.Result.Content;
        }

        public async Task<IReaction?> GetReactionInputAsync(ulong userId, ulong channelId, ulong messageId, ulong? guildId = null, TimeSpan? timeOut = null)
        {
            throw new NotImplementedException();
        }
        public async Task<IChannel?> GetChannelAsync(ulong userId, ulong channelId, ulong guildId, TimeSpan? timeOut = null)
        {
            throw new NotImplementedException();
        }
    }
}