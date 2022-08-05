using Remora.Rest.Core;

namespace Silk.Data.DTOs.Guilds.Config;

public record GuildLoggingConfigDTO
(
    int                ID,
    bool               LogMessageEdits,
    bool               LogMessageDeletes,
    bool               LogInfractions,
    bool               LogMemberJoins,
    bool               LogMemberLeaves,
    bool               UseWebhookLogging,
    Snowflake          GuildID,
    Snowflake?         FallbackChannelID,
    LoggingChannelDTO? Infractions,
    LoggingChannelDTO? MessageEdits,
    LoggingChannelDTO? MessageDeletes,
    LoggingChannelDTO? MemberJoins,
    LoggingChannelDTO? MemberLeaves,
    bool               UseMobileFriendlyLogging = true
);