using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Shared.Abstractions.DSharpPlus.Concrete
{
    public class Guild : IGuild
    {
        public ulong Id { get; private init; }
        public IReadOnlyList<IUser> Users { get; private init; }
        public IReadOnlyList<IChannel> Channels { get; private init; }
        public IReadOnlyList<IEmoji> Emojis { get; private init; }
        public IReadOnlyList<ulong> Roles { get; private init; }

        public static implicit operator Guild?(DiscordGuild? guild)
        {
            if (guild is null) return null;
            else
                return new()
                {
                    Id = guild.Id,
                    Users = guild.Members.Select(u => (User) (DiscordUser) u.Value).ToList().AsReadOnly(),
                    Channels = guild.Channels.Values.Select(c => (Channel) c).ToList().AsReadOnly(),
                    Emojis = guild.Emojis.Select(e => (Emoji) e.Value).ToList().AsReadOnly(),
                    Roles = guild.Roles.OrderBy(r => r.Value.Position).Select(r => r.Key).ToList().AsReadOnly()
                };
        }
    }
}