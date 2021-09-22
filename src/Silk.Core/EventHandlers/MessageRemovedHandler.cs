using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Silk.Core.Data.Entities;
using Silk.Core.Services.Data;

namespace Silk.Core.EventHandlers
{
	public sealed class MessageRemovedHandler
	{
		private readonly ConfigService _cache;
		public MessageRemovedHandler(ConfigService cache) => _cache = cache;

		public async Task MessageRemoved(DiscordClient c, MessageDeleteEventArgs e)
		{
			if (e.Message?.Author is null || e.Message is null) return; // Message isn't cached. //
			if (e.Message.Author.IsCurrent) return; // Self-evident.                            //
			if (e.Channel.IsPrivate) return; // Goes without saying.                           //
			_ = Task.Run(async () =>
			{
				GuildModConfigEntity config = await _cache.GetModConfigAsync(e.Guild.Id);

				if (!config.LogMessageChanges) return;
				if (config.LoggingChannel is 0) return;

				DiscordEmbed embed = GetEditEmbed(e);
				DiscordChannel channel = await c.GetChannelAsync(config.LoggingChannel);
				await channel.SendMessageAsync(embed).ConfigureAwait(false);
			});

		}

		private DiscordEmbedBuilder GetEditEmbed(MessageDeleteEventArgs e)
		{
			return new DiscordEmbedBuilder()
				.WithTitle("A message was deleted:")
				.WithDescription(
					$"Content: ```\n{e.Message.Content}```")
				.AddField("Channel", e.Channel.IsThread ? e.Channel.Parent.Mention : e.Channel.Mention, true)
				.AddField("Thread", e.Channel.IsThread ? e.Channel.Mention : "None", true)
				.AddField("\u200b", "\u200b", true)
				.AddField("Deleted at:",  Formatter.Timestamp(DateTime.Now), true)
				.AddField("Sent at:", Formatter.Timestamp(e.Message.Timestamp), true)
				.AddField("\u200b", "\u200b", true)
				.AddField("Message ID:", e.Message.Id.ToString(), true)
				.AddField("User ID:", e.Message.Author?.Id.ToString() ?? "I wasn't around at the time. Sorry!", true)
				.WithThumbnail(e.Message.Author?.AvatarUrl ?? string.Empty)
				.WithColor(DiscordColor.Red);
		}
	}
}