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
	public Snowflake GuildID { get; set; }

	/// <summary>
	///     Whether to log when messages are edited.
	/// </summary>
    public bool LogMessageEdits { get; set; }

	/// <summary>
	///     Whether to log when messages are deleted.
	/// </summary>
    public bool LogMessageDeletes { get; set; }

	/// <summary>
	///     Whether to log when infractions occur. Defaults to true..
	/// </summary>
	public bool LogInfractions { get; set; }

	/// <summary>
	///     Whether to log members joining or not.
	/// </summary>
    public bool LogMemberJoins { get; set; }

	/// <summary>
	///     Whether to log members leaving or not.
	/// </summary>
    public bool LogMemberLeaves { get; set; }

	/// <summary>
	/// If true, when logging attachments, they will chunked into four images per embed, instead of one image per embed.
	/// </summary>
	public bool UseMobileFriendlyLogging { get; set; } = true;

	/// <summary>
	///     The fallback channel to use if it's not specified for a specific scope.
	/// </summary>
	// TODO: Take this out of limbo
	[Column("fallback_logging_channel")]
    public Snowflake? FallbackChannelID { get; set; }

	/// <summary>
	///     Whether to use webhooks to log messages.
	/// </summary>
    public bool UseWebhookLogging { get; set; }

	/// <summary>
	/// Information about the channel to log infractions to.
	/// </summary>
	public LoggingChannelEntity? Infractions { get; set; }

	/// <summary>
	/// Information about the channel to log message edits to.
	/// </summary>
	public LoggingChannelEntity? MessageEdits { get; set; }
	
	/// <summary>
	/// Information about the channel to log message deletes to.
	/// </summary>
	public LoggingChannelEntity? MessageDeletes { get; set; }

	/// <summary>
	/// Information about the channel to log member joins to.
	/// </summary>
	public LoggingChannelEntity? MemberJoins { get; set; }

	/// <summary>
	/// Information about the channel to log member leaves to.
	/// </summary>
	public LoggingChannelEntity? MemberLeaves { get; set; }
}