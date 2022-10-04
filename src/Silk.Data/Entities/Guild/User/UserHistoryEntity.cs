using System;
using Remora.Rest.Core;
using Silk.Data.DTOs.Guilds.Users;

namespace Silk.Data.Entities;

/// <summary>
///     General history of a user.
/// </summary>
public class UserHistoryEntity
{
    /// <summary>
    ///     The Id of the user this history is reflective of.
    /// </summary>
    public Snowflake UserID { get; set; }
    
    public UserEntity User { get; set; }

    /// <summary>
    ///     The guild this history is related to.
    /// </summary>
    public Snowflake GuildID { get; set; }
    
    /// <summary>
    /// When this user joined.
    /// </summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>
    /// Whether this marker is a join or leave. True for join, false for leave.
    /// </summary>
    public bool IsJoin { get; set; }

    public static UserHistory ToDTO(UserHistoryEntity history)
        => new(history.GuildID, history.Date, history.IsJoin);
}