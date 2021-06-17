using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities.Bot;
using Silk.Shared.Constants;

namespace Silk.Core.Services.Bot
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
            InteractivityExtension? interactivity = GetInteractivityInternal(guildId);
            return await WaitForInputAsync(interactivity, userId, channelId, guildId, timeOut);
        }

        public async Task<DiscordMessage?> GetInputAsync(ulong userId, ulong channelId, ulong? guildId, TimeSpan? timeOut = null)
        {
            InteractivityExtension? interactivity = GetInteractivityInternal(guildId);
            return (await interactivity.WaitForMessageAsync(m => m.Author.Id == userId && m.Channel.Id == channelId)).Result!;
        }

        public async Task<DiscordReaction?> GetReactionInputAsync(ulong userId, ulong messageId, ulong? guildId = null, TimeSpan? timeOut = null)
        {
            InteractivityExtension? interactivity = GetInteractivityInternal(guildId);
            return (await interactivity.WaitForReactionAsync(r => r.Message.Id == messageId && r.User.Id == userId)).Result.Message.Reactions.Last();
        }

        public async Task<DiscordChannel?> GetChannelAsync(ulong userId, ulong channelId, ulong guildId, TimeSpan? timeOut = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<bool?> GetConfirmationAsync(DiscordMessage message, ulong userId, TimeSpan? timeOut = null)
        {
            InteractivityExtension? interactivity = GetInteractivityInternal(message.Channel.GuildId ?? 0);
            DiscordClient? client = interactivity.Client;
            DiscordEmoji? yes = DiscordEmoji.FromGuildEmote(client, Emojis.ConfirmId);
            DiscordEmoji? no = DiscordEmoji.FromGuildEmote(client, Emojis.DeclineId);

            await message.CreateReactionAsync(yes);
            await message.CreateReactionAsync(no);

            InteractivityResult<MessageReactionAddEventArgs> result = await interactivity.WaitForReactionAsync(r => r.Emoji == yes || r.Emoji == no && r.User.Id == userId);

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
            InteractivityResult<DiscordMessage> message = await interactivity.WaitForMessageAsync(m =>
            {
                bool isUser = m.Author.Id == userId;
                bool isChannel = m.ChannelId == channelId;
                bool isGuild = m.Channel.GuildId == guildId;

                if (guildId is null)
                    return isUser && isChannel;
                return isUser && isChannel && isGuild;
            }, timeOut);

            return message.TimedOut ? null : message.Result.Content;
        }
    }
}