using Remora.Rest.Core;

namespace Silk.Data.DTOs.Guilds.Config;

public record GuildLoggingConfig
{
    public int             Id                       { get; set; }
    public Snowflake       GuildID                  { get; set; }
    public Snowflake?      FallbackChannelID        { get; set; }
    public bool            LogMessageEdits          { get; set; }
    public bool            LogMessageDeletes        { get; set; }
    public bool            LogInfractions           { get; set; }
    public bool            LogMemberJoins           { get; set; }
    public bool            LogMemberLeaves          { get; set; }
    public bool            UseWebhookLogging        { get; set; }
    public bool            UseMobileFriendlyLogging { get; set; } = true;
    public LoggingChannel? Infractions              { get; set; }
    public LoggingChannel? MessageEdits             { get; set; }
    public LoggingChannel? MessageDeletes           { get; set; }
    public LoggingChannel? MemberJoins              { get; set; }
    public LoggingChannel? MemberLeaves             { get; set; }
}