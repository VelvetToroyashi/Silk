using System;
using Remora.Rest.Core;

namespace Silk.Data.DTOs.Guilds;

/// <summary>
/// An infraction.
/// </summary>
/// <param name="TargetID">The ID of the user this infraction affects.</param>
/// <param name="EnforcerID">The ID of the user that generated this infraction.</param>
/// <param name="GuildID">The ID of the guild this infraction was generated on.</param>
/// <param name="CreatedAt">When this infraction was created.</param>
/// <param name="ExpiresAt">When this infraction expires, if ever.</param>
/// <param name="Duration">How long this infraction lasts.</param>
/// <param name="CaseID">The ID of the infraction, per guild.</param>
/// <param name="Reason">The reason this infraction was created.</param>
/// <param name="Notified">Whether the user was notified about the infraction.</param>
/// <param name="Processed">Whether this infraction has been processed.</param>
/// <param name="Pardoned">Whether this infraction has been pardoned.</param>
public record InfractionDTO
(
    Snowflake       TargetID,
    Snowflake       EnforcerID,
    Snowflake       GuildID,
    DateTimeOffset  CreatedAt,
    DateTimeOffset? ExpiresAt,
    TimeSpan?       Duration,
    int             CaseID,
    string          Reason,
    bool            Notified,
    bool            Processed,
    bool            Pardoned
);
