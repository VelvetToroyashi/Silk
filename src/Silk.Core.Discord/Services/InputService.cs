using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Silk.Core.Discord.Constants;
using Silk.Core.Discord.Services.Interfaces;
using Silk.Core.Discord.Utilities.Bot;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Core.Discord.Services
{
    /// <inheritdoc cref="IInputService" />
    public class InputService : IInputService
    {
        private readonly DiscordShardedClient _client;
        private readonly BotConfig _config;

        public InputService(DiscordShardedClient client)
        {
            _client = client;
        }

        public async Task<string?> GetStringInputAsync(ulong userId, ulong channelId, ulong? guildId = null, TimeSpan? timeOut = null)
        {
            var interactivity = GetInteractivityInternal(guildId);
            return await WaitForInputAsync(interactivity, userId, channelId, guildId, timeOut);
        }

        public async Task<IReaction?> GetReactionInputAsync(ulong userId, ulong channelId, ulong messageId, ulong? guildId = null, TimeSpan? timeOut = null)
        {
            throw new NotImplementedException();
        }

        public async Task<IChannel?> GetChannelAsync(ulong userId, ulong channelId, ulong guildId, TimeSpan? timeOut = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<bool?> GetConfirmationAsync(IMessage message, ulong userId, TimeSpan? timeOut = null)
        {
            var interactivity = GetInteractivityInternal(message.GuildId ?? 0);
            var client = interactivity.Client;
            var yes = DiscordEmoji.FromGuildEmote(client, Emojis.ConfirmId);
            var no = DiscordEmoji.FromGuildEmote(client, Emojis.DeclineId);



            var result = await interactivity.WaitForReactionAsync(r => r.Emoji == yes || r.Emoji == no && r.User.Id == userId);

            if (result.TimedOut) return null;
            else return result.Result.Emoji == yes;
        }

        private InteractivityExtension GetInteractivityInternal(ulong? guildId)
        {
            if (guildId is null) { return _client.ShardClients[0].GetInteractivity(); } // DMs are handled on Shard 0 anyway. //
            DiscordClient guildShard = _client.ShardClients.Values.First(s => s.Guilds.Keys.Contains(guildId.Value));
            return guildShard.GetInteractivity();
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
                return isUser && isChannel && isGuild;
            }, timeOut);

            return message.TimedOut ? null : message.Result.Content;
        }
    }
}