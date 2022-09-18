using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Silk.Data.Entities;

/// <summary>
///     Represents the configuration for specific logging. e.g: Message edits, Message deletes, etc.
/// </summary>
public class LoggingChannelEntity
{
	/// <summary>
	///     Gets the numeric, auto-incrementing ID of the logging channel config.
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	///     The guild this configuration belongs to.
	/// </summary>
    public Snowflake GuildID { get; set; }

	/// <summary>
	///     The ID of the webhook to use for logging, if configured.
	/// </summary>
	public Snowflake WebhookID { get; set; }
	
	/// <summary>
	///     The token of the webhook to use for logging, if configured.
	/// </summary>
	public string WebhookToken { get; set; } = string.Empty;

	/// <summary>
	///     The channel to log to.
	/// </summary>
    public Snowflake ChannelID { get; set; }
}