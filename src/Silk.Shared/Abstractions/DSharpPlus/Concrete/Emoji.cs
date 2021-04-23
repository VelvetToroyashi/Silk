using System.Linq;
using DSharpPlus.Entities;
using Silk.Extensions.DSharpPlus;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Shared.Abstractions.DSharpPlus.Concrete
{
    public class Emoji : IEmoji
    {
        public ulong Id { get; init; }

        public string Name { get; init; }

        private readonly DiscordEmoji _emoji;
        public bool IsSharedEmoji()
        {
            var client = _emoji.GetClient();
            return client.Guilds.Values.Any(g => g.Emojis.Values.Contains(_emoji));
        }

        private Emoji(DiscordEmoji emoji)
        {
            _emoji = emoji;
            Id = emoji.Id;
            Name = emoji.Name;
        }

        public static explicit operator Emoji(DiscordEmoji emoji) => new(emoji);

    }
}