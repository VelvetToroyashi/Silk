using System;
using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;
using Silk.Data.DTOs.Guilds.Users;

namespace Silk.Data.Entities;

/// <summary>
///     General history of a user.
/// </summary>
[Table("user_histories")]
public class UserHistoryEntity
{
    /// <summary>
    ///     The Id of the user this history is reflective of.
    /// </summary>
    [Column("user_id")]
    public Snowflake UserID { get; set; }
    
    [Column("user")]
    public UserEntity User { get; set; }

    /// <summary>
    ///     The guild this history is related to.
    /// </summary>
    [Column("guild_id")]
    public Snowflake GuildID { get; set; }
    
    /// <summary>
    /// When this user joined.
    /// </summary>
    [Column("date")]
    public DateTimeOffset Date { get; set; }

    /// <summary>
    /// When this user left.
    /// </summary>
    [Column("is_join")]
    public bool IsJoin { get; set; }

    public static UserHistory ToDTO(UserHistoryEntity history)
        => new(history.GuildID, history.Date, history.IsJoin);
}