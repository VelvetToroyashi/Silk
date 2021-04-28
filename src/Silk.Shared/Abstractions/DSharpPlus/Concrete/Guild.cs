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
            Users = guild.Members.Select(u => (User) (DiscordUser) u.Value).ToList().AsReadOnly();
            Channels = guild.Channels.Values.Select(c => (Channel) c).ToList().AsReadOnly();
            Emojis = guild.Emojis.Select(e => (Emoji) e.Value).ToList().AsReadOnly();
            Roles = guild.Roles.OrderBy(r => r.Value.Position).Select(r => r.Key).ToList().AsReadOnly();
        }

        public static implicit operator Guild?(DiscordGuild? guild)
        {
            if (guild is null) return null;
            var isCached = Guilds.TryGetValue(guild.Id, out var g);
            g ??= new(guild);

            if (!isCached) Guilds.Add(guild.Id, g);

            return g;
        }
    }
}