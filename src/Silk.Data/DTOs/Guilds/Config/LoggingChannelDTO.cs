using Remora.Rest.Core;

namespace Silk.Data.DTOs.Guilds.Config;

public record LoggingChannelDTO
(
    int       ID,
    Snowflake GuildID,
    Snowflake ChannelID,
    Snowflake WebhookID,
    string    WebhookToken = ""
);