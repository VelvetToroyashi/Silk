using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Silk.Core.Data.Entities;

/// <summary>
///     General history of a user.
/// </summary>
[Table("user_histories")]
public class UserHistoryEntity
{
    public int Id { get; set; }
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
    ///     When this user initially joined.
    /// </summary>
    [Column("initial_join_date")]
    public DateTimeOffset JoinDate { get; set; }
    
    /// <summary>
    ///     Times this user joined.
    /// </summary>
    [Column("join_dates")]
    public List<DateTimeOffset> JoinDates { get; set; }

    /// <summary>
    ///     Times this user left.
    /// </summary>
    [Column("leave_dates")]
    public List<DateTimeOffset> LeaveDates { get; set; }
}