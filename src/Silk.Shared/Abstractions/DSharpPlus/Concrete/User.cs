using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using Silk.Extensions.DSharpPlus;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Shared.Abstractions.DSharpPlus.Concrete
{
    public class User : IUser
    {
        public ulong Id { get; private init; }
        public DateTimeOffset CreationTimestamp { get; }
        public DateTimeOffset? JoinedTimestamp { get; private init; }
        public bool IsFromGuild { get; }
        public bool HasSharedGuild => _member?.GetClient().Guilds.SelectMany(g => g.Value.Members.Keys).Any(m => m == Id) ?? false;

        public IReadOnlyList<ulong>? Roles { get; private init; }

        private readonly DiscordMember? _member;

        private User(DiscordUser user)
        {
            Id = user.Id;
            CreationTimestamp = user.CreationTimestamp;

            if (user is DiscordMember member)
            {
                JoinedTimestamp = member.JoinedAt;
                IsFromGuild = true;
                Roles = member.Roles.Select(r => r.Id).ToList().AsReadOnly();
                _member = member;
            }
        }

        public static implicit operator User(DiscordUser u) => new(u);
    }
}