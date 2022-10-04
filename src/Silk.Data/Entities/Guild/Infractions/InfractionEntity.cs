using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Remora.Rest.Core;
using Silk.Data.DTOs.Guilds;

namespace Silk.Data.Entities;

public class InfractionEntity
{
    public int ID { get; set; } //Requisite Id for DB purposes
    
    /// <summary>
    ///     The Id of the target this infraction belongs to.
    /// </summary>
    public Snowflake TargetID { get; set; }
    
    /// <summary>
    ///     The Id of the target that gave this infraction; Auto-Mod infractions will default to the bot.
    /// </summary>
    public Snowflake EnforcerID { get; set; } //Who gave this infraction
    
    /// <summary>
    ///     The Id of the guild this infraction was given on.
    /// </summary>
    public Snowflake GuildID { get; set; }

    /// <summary>
    ///     The guild-specific case Id of this infraction.
    /// </summary>
    public int CaseNumber { get; set; }

    /// <summary>
    ///     The guild this infraction was given on.
    /// </summary>
    public GuildEntity Guild { get; set; }

    /// <summary>
    ///     The Target this infraction was given to.
    /// </summary>
    public UserEntity Target { get; set; }
    
    /// <summary>
    ///     The reason this infraction was given. Infractions added by Auto-Mod will be prefixed with "[AUTO-MOD]".
    /// </summary>
    public string Reason { get; set; } = "Not given."; // Why was this infraction given
    
    /// <summary>
    /// Gets whether the user was notified of the infraction, i.e. "Was the user DM'd?".
    /// </summary>
    public bool UserNotified { get; set; } = false; //Has the user been notified of this infraction?
    
    /// <summary>
    ///     Whether this infraction has been processed.
    /// </summary>
    public bool Processed { get; set; }
    
    /// <summary>
    ///     The time this infraction was added.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } //When it happened
    
    /// <summary>
    ///     The type of infraction.
    /// </summary>
    public InfractionType Type { get; set; } //What happened

    /// <summary>
    ///     Whether this infraction has been escalated..
    /// </summary>
    public bool Escalated { get; set; }

    /// <summary>
    ///     Whether this is an active infraction and/or this infraction counts toward any auto-incrementing severity of infractions.
    ///     Infraction will still hold on the target's record but is not held against them if set to false.
    /// </summary>
    public bool AppliesToTarget { get; set; } = true; // Used for infraction service to determine whether to escalate or not //

    /// <summary>
    ///     When this infraction is set to expire. Resolves to null
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    ///     How long this infraction is lasts.
    /// </summary>
    [NotMapped]
    public TimeSpan? Duration => !ExpiresAt.HasValue ? null : ExpiresAt.Value - CreatedAt;

    [return: NotNullIfNotNull("infraction")]
    public static Infraction? ToDTO(InfractionEntity? infraction)
        => infraction is null ? null : new(infraction.TargetID, infraction.EnforcerID, infraction.GuildID, infraction.Type, infraction.CreatedAt, infraction.ExpiresAt, infraction.Duration, infraction.CaseNumber, infraction.Reason, infraction.UserNotified, infraction.Processed, !infraction.AppliesToTarget);
}