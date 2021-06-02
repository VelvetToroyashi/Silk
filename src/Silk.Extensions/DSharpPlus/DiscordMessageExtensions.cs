#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

#endregion

namespace Silk.Extensions.DSharpPlus
{
    public static class DiscordMessageExtensions
    {
        public static async Task<DiscordEmoji[]> CreateReactionsAsync(this DiscordMessage msg, params ulong[] emojis)
        {
            DiscordClient client = msg.GetClient();
            var emojiArray = new DiscordEmoji[emojis.Length];
            for (var i = 0; i < emojis.Length; i++)
            {
                ulong e = emojis[i];
                DiscordEmoji emoji = DiscordEmoji.FromGuildEmote(client, e);
                await msg.CreateReactionAsync(emoji);
                emojiArray[i] = emoji;
            }
            return emojiArray;
        }

        public static async Task DeleteAsync(this IEnumerable<DiscordMessage> messageCollection)
        {
            if (messageCollection is null)
                throw new ArgumentNullException(nameof(messageCollection));

            DiscordChannel channel = messageCollection.First().Channel;
            await channel.DeleteMessagesAsync(messageCollection);
        }
    }
}