using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.DTOs.Guilds.Config;

public record ExemptionDTO
(
    int ID,
    ExemptionCoverage Coverage,
    ExemptionTarget Target,
    Snowflake TargetID,
    Snowflake GuildId
);