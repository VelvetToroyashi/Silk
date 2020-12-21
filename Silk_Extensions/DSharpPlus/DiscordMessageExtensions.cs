using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace SilkBot.Extensions.DSharpPlus
{
    public static class DiscordMessageExtensions
    {

        public static async Task DeleteAsync(this IEnumerable<DiscordMessage> messageCollection)
        {
            if (messageCollection is null) 
                throw new ArgumentNullException(nameof(messageCollection), "Cannot be null!");
            
            DiscordChannel channel = messageCollection.First().Channel;
            await channel.DeleteMessagesAsync(messageCollection);
        }
        
        public static async IAsyncEnumerator<DiscordMessage> GetAsyncEnumerator(this DiscordChannel channel)
        {
            bool hasMorePages = true;
            DiscordMessage last = null;
            do
            {
                var messages = last != null ? await channel.GetMessagesBeforeAsync(last.Id, 100) : await channel.GetMessagesAsync(100);
                if (messages.Count < 100)
                    hasMorePages = false;

                foreach (var message in messages)
                {
                    last = message;
                    yield return message;
                }

            } while (hasMorePages);
        }
    }
}