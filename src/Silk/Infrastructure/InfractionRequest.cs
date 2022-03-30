using System;
using OneOf;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk;

/// <summary>
/// Represents a request to submit an infraction.
/// </summary>
/// <param name="Notify">Whether the user should be notified of this infraction.</param>
/// <param name="Reason">The reason to attach to the infraction.</param>
/// <param name="UserReason">The reason to present the user; if set to null, <see cref="Reason"/> will be used.</param>
/// <param name="Duration">The optional duration to apply to the infraction.</param>
/// <param name="GuildID">The ID of the guild the infraction is for.</param>
/// <param name="Type">The type of infraction. If set to null, the current infraction step is used if configured, otherwise a strike.</param>
/// <param name="Target">The target of the infraction.</param>
/// <param name="Moderator">The moderator responsible for the infraction. If set to null, it is assumed to be an auto-mod infraction.</param>
public record struct InfractionRequest
(
    bool                     Notify,
    string                   Reason,
    string?                  UserReason,
    TimeSpan?                Duration,
    Snowflake                GuildID,
    InfractionType?          Type,
    OneOf<IUser, Snowflake>  Target,
    OneOf<IUser, Snowflake>? Moderator
);
