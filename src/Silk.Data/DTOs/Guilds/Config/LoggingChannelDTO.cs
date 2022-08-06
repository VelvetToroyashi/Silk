using Remora.Rest.Core;

namespace Silk.Data.DTOs.Guilds.Config;

public record LoggingChannelDTO
{
    public int       Id           { get; set; }
    public Snowflake GuildID      { get; set; }
    public Snowflake WebhookID    { get; set; }
    public string    WebhookToken { get; set; } = string.Empty;
    public Snowflake ChannelID    { get; set; }
}