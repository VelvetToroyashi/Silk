using System.Collections.Generic;
using Remora.Rest.Core;

namespace Silk.Data.DTOs.Guilds.Users;

public record User
(
    Snowflake                     ID,
    string?                       TimezoneID,
    bool                          ShareTimezone,
    IReadOnlyList<Snowflake>      Guilds,
    IReadOnlyList<UserHistory> History,
    IReadOnlyList<Infraction>  Infractions
);