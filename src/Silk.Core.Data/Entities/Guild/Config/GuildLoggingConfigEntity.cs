using System.ComponentModel.DataAnnotations.Schema;

namespace Silk.Core.Data.Entities
{
	/// <summary>
	/// Various bits of configuration related to logging.
	/// </summary>
	public class GuildLoggingConfigEntity
	{
		/// <summary>
		/// Gets the numeric, auto-incrementing ID of the guild logging config.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// The guild this configuration is for.
		/// </summary>
		[Column("guild_id")]
		public ulong GuildId { get; set; }

		/// <summary>
		/// The fallback channel to use if it's not specified for a specific scope.
		/// </summary>
		[Column("fallback_logging_channel")]
		public ulong? FallbackLoggingChannel { get; set; }

		/// <summary>
		/// Whether to use webhooks to log messages.
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
}