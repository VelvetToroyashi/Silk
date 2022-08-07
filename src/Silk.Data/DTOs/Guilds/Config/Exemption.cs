using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.DTOs.Guilds.Config;

public record Exemption
{
    public int               Id         { get; set; }
    public ExemptionCoverage Coverage   { get; set; }
    public ExemptionTarget   TargetType { get; set; }
    public Snowflake         TargetID   { get; set; }
    public Snowflake         GuildID    { get; set; }
}