using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Silk.Data.Entities;

/// <summary>
///     Represents the configuration for specific logging. e.g: Message edits, Message deletes, etc.
/// </summary>
[Table("logging_channels")]
public class LoggingChannelEntity
{
	/// <summary>
	///     Gets the numeric, auto-incrementing ID of the logging channel config.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	///     The guild this configuration belongs to.
	/// </summary>
	[Column("guild_id")]
    public Snowflake GuildID { get; set; }

	/// <summary>
	///     The ID of the webhook to use for logging, if configured.
	/// </summary>
	[Column("webhook_id")]
	public Snowflake WebhookID { get; set; }
	
	/// <summary>
	///     The token of the webhook to use for logging, if configured.
	/// </summary>
	[Column("webhook_token")]
	public string WebhookToken { get; set; } = string.Empty;

	/// <summary>
	///     The channel to log to.
	/// </summary>
	[Column("channel_id")]
    public Snowflake ChannelID { get; set; }
}