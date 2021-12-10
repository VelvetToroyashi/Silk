﻿using System;
using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Silk.Core.Data.Entities;

public class InfractionEntity
{
    public int Id { get; set; } //Requisite Id for DB purposes
    /// <summary>
    ///     The Id of the target this infraction belongs to.
    /// </summary>
    [Column("target_id")]
    public Snowflake TargetID { get; set; }
    
    /// <summary>
    ///     The Id of the target that gave this infraction; Auto-Mod infractions will default to the bot.
    /// </summary>
    [Column("enforcer_id")]
    public Snowflake EnforcerID { get; set; } //Who gave this infraction
    
    /// <summary>
    ///     The Id of the guild this infraction was given on.
    /// </summary>
    [Column("guild_id")]
    public Snowflake GuildID { get; set; }

    /// <summary>
    ///     The guild-specific case Id of this infraction.
    /// </summary>
    [Column("case_id")]
    public int CaseNumber { get; set; }

    /// <summary>
    ///     The guild this infraction was given on.
    /// </summary>
    [Column("guild")]
    public GuildEntity Guild { get; set; }

    /// <summary>
    ///     The Target this infraction was given to.
    /// </summary>
    [Column("target")]
    public UserEntity Target { get; set; }
    
    /// <summary>
    ///     The reason this infraction was given. Infractions added by Auto-Mod will be prefixed with "[AUTO-MOD]".
    /// </summary>
    [Column("reason")]
    public string Reason { get; set; } = "Not given."; // Why was this infraction given

    /// <summary>
    ///     Whether this infraction has been processed.
    /// </summary>
    [Column("processed")]
    public bool Processed { get; set; }
    
    /// <summary>
    ///     The time this infraction was added.
    /// </summary>
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } //When it happened
    
    /// <summary>
    ///     The type of infraction.
    /// </summary>
    [Column("type")]
    public InfractionType Type { get; set; } //What happened

    /// <summary>
    ///     Whether this infraction has been escalated..
    /// </summary>
    [Column("escalated")]
    public bool Escalated { get; set; }

    /// <summary>
    ///     Whether this is an active infraction and/or this infraction counts toward any auto-incrementing severity of infractions.
    ///     Infraction will still hold on the target's record but is not held against them if set to false.
    /// </summary>
    [Column("active")]
    public bool AppliesToTarget { get; set; } = true; // Used for infraction service to determine whether to escalate or not //

    /// <summary>
    ///     When this infraction is set to expire. Resolves to null
    /// </summary>
    [Column("expires_at")]
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    ///     How long this infraction is lasts.
    /// </summary>
    [NotMapped]
    public TimeSpan? Duration => !ExpiresAt.HasValue ? null : ExpiresAt.Value - CreatedAt;
}