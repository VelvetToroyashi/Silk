using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Shared.Abstractions.DSharpPlus.Concrete
{
    public class Guild : IGuild
    {
        public ulong Id { get; init; }
        public IReadOnlyList<IUser> Users { get; init; }
        public IReadOnlyList<IChannel> Channels { get; init; }

        public static explicit operator Guild?(DiscordGuild? guild) =>
            guild is null ? null
                : new()
                {
                    Id = guild.Id,
                    Users = guild.Members.Select(u => (User) (DiscordUser) u.Value).ToList().AsReadOnly(),
                    Channels = guild.Channels.Values.Select(c => new Channel() {Id = c.Id}).ToList().AsReadOnly()
                };
    }
}