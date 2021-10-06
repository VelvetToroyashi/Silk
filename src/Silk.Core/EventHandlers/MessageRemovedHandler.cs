using System;
using System.IO;
using System.Linq;
using System.Net.Http;
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
		private readonly HttpClient _client;
		public MessageRemovedHandler(ConfigService cache, HttpClient client)
		{
			_cache = cache;
			_client = client;
		}

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

				DiscordEmbedBuilder editEmbed = GetEditEmbed(e);
				DiscordChannel channel = await c.GetChannelAsync(config.LoggingChannel);
				
				if (e.Message.Attachments.Count is 1)
				{
					var attachment = e.Message.Attachments.First();
				
					var stream = await GetSingleAttatchmentAsync(e.Message);

					if (stream is null)
					{
						await channel.SendMessageAsync(editEmbed).ConfigureAwait(false);
						return;
					}
					
					var builder = new DiscordMessageBuilder();

					builder.WithFile(attachment.FileName, stream);

					var attachmentEmbed = new DiscordEmbedBuilder()
						.WithColor(DiscordColor.Red)
						.WithTitle($"Attachment 1 for {e.Message.Id}:")
						.AddField("File Name:", attachment.FileName, true)
						.AddField("File Size:", $"{attachment.FileSize / 1024} kb", true)
						.WithImageUrl($"attachment://{attachment.FileName}");

					builder.AddEmbeds(new DiscordEmbed[] { editEmbed, attachmentEmbed });
						
					await channel.SendMessageAsync(builder);
				}
			});

		}

		private async Task<Stream?> GetSingleAttatchmentAsync(DiscordMessage message)
		{
			var ret = await _client.GetAsync(message.Attachments.First().Url);

			if (!ret.IsSuccessStatusCode)
				return null;

			return await ret.Content.ReadAsStreamAsync();
		}
		
		private DiscordEmbedBuilder GetEditEmbed(MessageDeleteEventArgs e)
		{
			return new DiscordEmbedBuilder()
				.WithTitle("A message was deleted:")
				.WithDescription(
					$"Content: {(string.IsNullOrEmpty(e.Message.Content) ? "Message did not contain content." : $"```\n{e.Message.Content}```")}")
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