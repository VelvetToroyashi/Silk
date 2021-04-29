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
        public ulong Id { get; }
        public bool IsBot { get; }
        public string Mention => Nickname is null ? $"<@{Id}>" : $"<@!{Id}>";
        public string Username { get; }
        public string? Nickname => _member?.Nickname;
        public DateTimeOffset CreationTimestamp { get; }
        public DateTimeOffset? JoinedTimestamp { get; }
        public bool IsFromGuild { get; }
        public bool HasSharedGuild => _member?.GetClient().Guilds.SelectMany(g => g.Value.Members.Keys).Any(m => m == Id) ?? false;

        public IReadOnlyList<ulong>? Roles { get; }

        private readonly DiscordMember? _member;

        internal User(DiscordUser user, bool caching)
        {
            Id = user.Id;
            Username = user.Username;
            IsBot = user.IsBot;
            CreationTimestamp = user.CreationTimestamp;

            if (user is DiscordMember member)
            {
                JoinedTimestamp = member.JoinedAt;
                IsFromGuild = true;

                Roles = member.Roles.Select(r => r.Id).ToList();
                _member = member;
            }
        }

        public static implicit operator User(DiscordUser u) => new(u, false);
    }
}