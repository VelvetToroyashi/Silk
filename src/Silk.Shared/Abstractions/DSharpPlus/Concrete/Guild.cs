using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Shared.Abstractions.DSharpPlus.Concrete
{
    public class Guild : IGuild
    {
        public ulong Id => _guild.Id;
        public IDictionary<ulong, IUser> Users { get; }
        public IDictionary<ulong, IChannel> Channels { get; }
        public IReadOnlyList<IEmoji> Emojis { get; }
        public IReadOnlyList<ulong> Roles { get; }


        private readonly DiscordGuild _guild;
        private Guild(DiscordGuild guild)
        {
            if (guild is null!) return;


            Emojis = guild.Emojis.Select(e => (Emoji) e.Value).ToList();
            Roles = guild.Roles.OrderBy(r => r.Value.Position).Select(r => r.Key).ToList();

            _guild = guild;
        }

        public static implicit operator Guild(DiscordGuild guild) => new(guild);
    }
}