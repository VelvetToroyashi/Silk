using System;
using Remora.Rest.Core;

namespace Silk.Data.DTOs.Guilds.Users;

/// <summary>
/// Represents a record of a user joining and leaving a guild.
/// </summary>
/// <param name="GuildID">The ID of the guild that was joined.</param>
/// <param name="Joined">The timestamp the user joined.</param>
/// <param name="IsJoin">Whether this history was a join..</param>
public record UserHistory
(
    Snowflake      GuildID,
    DateTimeOffset Joined,
    bool           IsJoin
);