using DSharpPlus;
using DSharpPlus.Entities;
using System.Collections.Generic;

namespace SilkBot.Utilities
{
    public sealed class DiscordEmojiCreationService
    {
        public DiscordEmoji GetEmoji(string name)
        {
            return DiscordEmoji.FromName(Bot.Instance.Client, name);
        }
        public IEnumerable<DiscordEmoji> GetEmoji(params string[] names)
        {
            foreach(var emojiName in names)
            {
                yield return DiscordEmoji.FromName(Bot.Instance.Client, emojiName);
            }
        }
    }
}
