using System.Collections.Generic;
using DSharpPlus.Entities;

namespace SilkBot.Extensions
{
    public static class DiscordMessageExtensions
    {
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