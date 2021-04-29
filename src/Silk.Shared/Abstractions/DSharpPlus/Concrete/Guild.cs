using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Shared.Abstractions.DSharpPlus.Concrete
{
    public class Guild : IGuild
    {
        public ulong Id { get; }
        public IReadOnlyList<IUser> Users { get; }
        public IReadOnlyList<IChannel> Channels { get; internal set; }
        public IReadOnlyList<IEmoji> Emojis { get; }
        public IReadOnlyList<ulong> Roles { get; }

        internal static Dictionary<ulong, Guild> Guilds { get; } = new();

        private Guild(DiscordGuild guild)
        {
            Id = guild.Id;
            Emojis = guild.Emojis.Select(e => (Emoji) e.Value).ToList().AsReadOnly();
            Roles = guild.Roles.OrderBy(r => r.Value.Position).Select(r => r.Key).ToList().AsReadOnly();
        }

        public static implicit operator Guild?(DiscordGuild? guild) => guild is null ? null : new(guild);
    }
}