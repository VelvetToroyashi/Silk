using Remora.Rest.Core;

namespace Silk.Data.DTOs.Guilds.Config;

public record GuildLoggingConfigDTO
{
    public int                Id                       { get; set; }
    public Snowflake          GuildID                  { get; set; }
    public Snowflake?         FallbackChannelID        { get; set; }
    public bool               LogMessageEdits          { get; set; }
    public bool               LogMessageDeletes        { get; set; }
    public bool               LogInfractions           { get; set; }
    public bool               LogMemberJoins           { get; set; }
    public bool               LogMemberLeaves          { get; set; }
    public bool               UseWebhookLogging        { get; set; }
    public bool               UseMobileFriendlyLogging { get; set; } = true;
    public LoggingChannelDTO? Infractions              { get; set; }
    public LoggingChannelDTO? MessageEdits             { get; set; }
    public LoggingChannelDTO? MessageDeletes           { get; set; }
    public LoggingChannelDTO? MemberJoins              { get; set; }
    public LoggingChannelDTO? MemberLeaves             { get; set; }
}