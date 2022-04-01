using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Silk.Data.Entities;

/// <summary>
///     Various bits of configuration related to logging.
/// </summary>
[Table("guild_logging_configs")]
public class GuildLoggingConfigEntity
{
	/// <summary>
	///     Gets the numeric, auto-incrementing ID of the guild logging config.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	///     The guild this configuration is for.
	/// </summary>
	[Column("guild_id")]
    public Snowflake GuildID { get; set; }


	/// <summary>
	///     Whether to log when messages are edited.
	/// </summary>
	[Column("log_message_edits")]
    public bool LogMessageEdits { get; set; }

	/// <summary>
	///     Whether to log when messages are deleted.
	/// </summary>
	[Column("log_message_deletes")]
    public bool LogMessageDeletes { get; set; }

	/// <summary>
	///     Whether to log when infractions occur. Defaults to true..
	/// </summary>
	[Column("log_infractions")]
    public bool LogInfractions { get; set; }

	/// <summary>
	///     Whether to log members joining or not.
	/// </summary>
	[Column("log_member_joins")]
    public bool LogMemberJoins { get; set; }

	/// <summary>
	///     Whether to log members leaving or not.
	/// </summary>
	[Column("log_member_leaves")]
    public bool LogMemberLeaves { get; set; }

	/// <summary>
	/// If true, when logging attachments, they will chunked into four images per embed, instead of one image per embed.
	/// </summary>
	[Column("use_mobile_friendly_logging")]
	public bool UseMobileFriendlyLogging { get; set; } = true;

	/// <summary>
	///     The fallback channel to use if it's not specified for a specific scope.
	/// </summary>
	[Column("fallback_logging_channel")]
    public Snowflake? FallbackChannelID { get; set; }

	/// <summary>
	///     Whether to use webhooks to log messages.
	/// </summary>
	[Column("use_webhook_logging")]
    public bool UseWebhookLogging { get; set; }

	/// <summary>
	/// Information about the channel to log infractions to.
	/// </summary>
    [Column("infractions_channel")]
    public LoggingChannelEntity? Infractions { get; set; }

	/// <summary>
	/// Information about the channel to log message edits to.
	/// </summary>
    [Column("edits_channel")]
    public LoggingChannelEntity? MessageEdits { get; set; }
	
	/// <summary>
	/// Information about the channel to log message deletes to.
	/// </summary>
    [Column("deletes_channel")]
    public LoggingChannelEntity? MessageDeletes { get; set; }

	/// <summary>
	/// Information about the channel to log member joins to.
	/// </summary>
    [Column("joins_channel")]
    public LoggingChannelEntity? MemberJoins { get; set; }

	/// <summary>
	/// Information about the channel to log member leaves to.
	/// </summary>
    [Column("leaves_channel")]
    public LoggingChannelEntity? MemberLeaves { get; set; }
}