using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.DTOs.Guilds.Config;

public record GuildGreetingDTO
{
    public int            Id         { get; set; }
    public Snowflake      GuildID    { get; set; }
    public string         Message    { get; set; } = string.Empty;
    public GreetingOption Option     { get; set; }
    public Snowflake      ChannelID  { get; set; }
    public Snowflake?     MetadataID { get; set; }
}