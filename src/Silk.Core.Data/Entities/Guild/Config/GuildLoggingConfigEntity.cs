using System.ComponentModel.DataAnnotations.Schema;

namespace Silk.Core.Data.Entities;

/// <summary>
///     Various bits of configuration related to logging.
/// </summary>
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
    public ulong GuildId { get; set; }


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
	///     The fallback channel to use if it's not specified for a specific scope.
	/// </summary>
	[Column("fallback_logging_channel")]
    public ulong? FallbackLoggingChannel { get; set; }

	/// <summary>
	///     Whether to use webhooks to log messages.
	/// </summary>
	[Column("use_webhook_logging")]
    public bool UseWebhookLogging { get; set; }

    [Column("infractions_channel")]
    public LoggingChannelEntity? Infractions { get; set; }

    [Column("edits_channel")]
    public LoggingChannelEntity? MessageEdits { get; set; }

    [Column("deletes_channel")]
    public LoggingChannelEntity? MessageDeletes { get; set; }

    [Column("joins_channel")]
    public LoggingChannelEntity? MemberJoins { get; set; }

    [Column("leaves_channel")]
    public LoggingChannelEntity? MemberLeaves { get; set; }
}