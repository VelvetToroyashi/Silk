using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities.Bot;
using Silk.Shared.Constants;

namespace Silk.Core.Services
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

        public async Task<DiscordMessage?> GetInputAsync(ulong userId, ulong channelId, ulong? guildId, TimeSpan? timeOut = null)
        {
            var interactivity = GetInteractivityInternal(guildId);
            return (await interactivity.WaitForMessageAsync(m => m.Author.Id == userId && m.Channel.Id == channelId)).Result!;
        }

        public async Task<DiscordReaction?> GetReactionInputAsync(ulong userId, ulong messageId, ulong? guildId = null, TimeSpan? timeOut = null)
        {
            var interactivity = GetInteractivityInternal(guildId);
            return (await interactivity.WaitForReactionAsync(r => r.Message.Id == messageId && r.User.Id == userId)).Result.Message.Reactions.Last();
        }

        public async Task<DiscordChannel?> GetChannelAsync(ulong userId, ulong channelId, ulong guildId, TimeSpan? timeOut = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<bool?> GetConfirmationAsync(DiscordMessage message, ulong userId, TimeSpan? timeOut = null)
        {
            var interactivity = GetInteractivityInternal(message.Channel.GuildId ?? 0);
            var client = interactivity.Client;
            var yes = DiscordEmoji.FromGuildEmote(client, Emojis.ConfirmId);
            var no = DiscordEmoji.FromGuildEmote(client, Emojis.DeclineId);

            await message.CreateReactionAsync(yes);
            await message.CreateReactionAsync(no);

            var result = await interactivity.WaitForReactionAsync(r => r.Emoji == yes || r.Emoji == no && r.User.Id == userId);

            if (result.TimedOut) return null;
            return result.Result.Emoji == yes;
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