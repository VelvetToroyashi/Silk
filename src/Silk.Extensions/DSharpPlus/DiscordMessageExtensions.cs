#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;

#endregion

namespace Silk.Extensions.DSharpPlus
{
    public static class DiscordMessageExtensions
    {

        public static async Task DeleteAsync(this IEnumerable<DiscordMessage> messageCollection)
        {
            if (messageCollection is null)
                throw new ArgumentNullException(nameof(messageCollection));

            DiscordChannel channel = messageCollection.First().Channel;
            await channel.DeleteMessagesAsync(messageCollection);
        }
    }
}