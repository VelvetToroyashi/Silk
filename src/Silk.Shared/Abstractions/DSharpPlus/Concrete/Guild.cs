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
        public IReadOnlyList<IChannel> Channels { get; }
        public IReadOnlyList<IEmoji> Emojis { get; }
        public IReadOnlyList<ulong> Roles { get; }

        internal static Dictionary<ulong, Guild> Guilds { get; } = new();

        private Guild(DiscordGuild guild)
        {
            Id = guild.Id;
            Users = guild.Members.Values.Select(m => new User(m, true)).ToList();
            Channels = guild.Channels.Values.Select(c => new Channel(c, true)).ToList();
            Emojis = guild.Emojis.Select(e => (Emoji) e.Value).ToList();
            Roles = guild.Roles.OrderBy(r => r.Value.Position).Select(r => r.Key).ToList();

            Guilds.Add(guild.Id, this);
        }


        public static implicit operator Guild?(DiscordGuild? guild)
        {
            if (guild is null) return null;
            if (Guilds.TryGetValue(guild.Id, out var g)) return g;

            return new(guild);
        }
    }
}