using System.Reflection;
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
            if (Id is 0) return true; // Default Emoji //
            var client = _emoji.GetClient();
            return DiscordEmoji.TryFromGuildEmote(client, Id, out _);
        }

        private Emoji(DiscordEmoji emoji)
        {
            _emoji = emoji;
            Id = emoji.Id;
            Name = emoji.Name;
        }

        public override string ToString() => _emoji.ToString();

        public static implicit operator Emoji(DiscordEmoji emoji) => new(emoji);
        public static implicit operator DiscordEmoji(Emoji emoji) =>
            (typeof(Emoji).GetField(nameof(_emoji), BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(emoji) as DiscordEmoji)!;
    }
}