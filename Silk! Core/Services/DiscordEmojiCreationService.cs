using DSharpPlus;
using DSharpPlus.Entities;
using System.Collections.Generic;

namespace SilkBot.Utilities
{
    public sealed class DiscordEmojiCreationService
    {
        private readonly DiscordClient _client;

        public DiscordEmojiCreationService(DiscordClient client) => _client = client;
        public DiscordEmoji GetEmoji(string name)
        {
            return DiscordEmoji.FromName(_client, name);
        }
        public IEnumerable<DiscordEmoji> GetEmoji(params string[] names)
        {
            foreach(var emojiName in names)
            {
                yield return DiscordEmoji.FromName(_client, emojiName);
            }
        }
    }
}
