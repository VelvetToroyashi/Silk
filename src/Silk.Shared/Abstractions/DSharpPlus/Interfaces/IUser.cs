using System;
using System.Collections.Generic;

namespace Silk.Shared.Abstractions.DSharpPlus.Interfaces
{
    public interface IUser
    {
        /// <summary>
        /// The snowflake Id that represents this user.
        /// </summary>
        public ulong Id { get; }

        /// <summary>
        /// When the user's account was created.
        /// </summary>
        public DateTimeOffset CreationTimestamp { get; }

        /// <summary>
        /// When the user joined the guild, if applicable.
        /// </summary>
        public DateTimeOffset? JoinedTimestamp { get; }

        /// <summary>
        /// Whether or not this user object belongs to a server,
        /// in which case <see cref="Roles"/> will be non-null.
        /// </summary>
        public bool IsFromGuild { get; }

        /// <summary>
        /// Whether the user has any common guilds.
        /// </summary>
        public bool HasSharedGuild { get; }

        /// <summary>
        /// The roles the user has. <see landword="null"/> if
        /// the object originated from DMs, out outside the current guild.
        /// </summary>
        public IReadOnlyList<ulong>? Roles { get; }
    }
}