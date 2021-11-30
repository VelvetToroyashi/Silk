using System.ComponentModel.DataAnnotations.Schema;

namespace Silk.Core.Data.Entities
{
	/// <summary>
	///     Represents the configureation for specific logging. e.g: Message edits, Message deletes, etc.
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
		[Column("guild_id")]
		public ulong GuildId { get; set; }

		/// <summary>
		///     The ID of the webhook to use for logging, if configured.
		/// </summary>
		[Column("webhook_id")]
		public ulong WebhookId { get; set; }

		/// <summary>
		///     The token of the webhook to use for logging, if configured.
		/// </summary>
		[Column("webhook_token")]
		public string WebhookToken { get; set; }

		/// <summary>
		///     The channel to log to.
		/// </summary>
		[Column("channel_id")]
		public ulong ChannelId { get; set; }
	}
}