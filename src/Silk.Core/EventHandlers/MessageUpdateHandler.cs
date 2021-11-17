using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.Entities;
using Silk.Core.Services.Data;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.EventHandlers
{
	public sealed class MessageUpdateHandler
	{
		private readonly ConfigService _cache;
		private readonly HttpClient _client;
		private readonly ILogger<MessageUpdateHandler> _logger;

		private readonly DiscordWebhookClient _whClient = new();
		
		public MessageUpdateHandler(DiscordClient dclient, ConfigService cache, HttpClient client, ILogger<MessageUpdateHandler> logger)
		{
			_cache = cache;
			_client = client;
			_logger = logger;

			dclient.MessageDeleted += MessageRemoved;
			dclient.MessageUpdated += MessageUpdated;
		}
		
		private async Task MessageUpdated(DiscordClient c, MessageUpdateEventArgs e)
		{
			if (e.Message.Author.IsBot) return;
			if (e.Message.Content == e.MessageBefore?.Content) return;
            if (e.Message.Author.IsCurrent) return;
            if (string.IsNullOrEmpty(e.Message.Content) || string.IsNullOrEmpty(e.MessageBefore?.Content)) return;
            
            _ = Task.Run(async () =>
            {
	            GuildModConfigEntity config = await _cache.GetModConfigAsync(e.Guild.Id);

	            if (!config.LogMessageChanges) return;
	            if (config.LoggingChannel is 0) return;

			    if (e.Message.Content.Length > 1950 || e.MessageBefore.Content.Length > 1950)
			    {
			        var embed = AddLoggingFields(new ())
			            .WithTitle("A message was edited:")
			            .WithDescription("The message was too big, and have been added to a seperate embed.")
			            .WithColor(DiscordColor.Orange);

			        var contentBefore = new DiscordEmbedBuilder()
			            .WithTitle($"Content before:")
			            .WithColor(DiscordColor.Orange)
			            .WithDescription(e.MessageBefore.Content ?? "Message did not previously have content");
			        
			        var contentAfter = new DiscordEmbedBuilder()
			            .WithTitle($"Content after:")
			            .WithColor(DiscordColor.Orange)
			            .WithDescription(e.Message.Content);



			        if (!e.Guild.Channels.TryGetValue(config.LoggingChannel, out var channel))
			        {
			            _logger.LogWarning("Configured logging channel was not present in guild: {Guild}", e.Guild.Id);
			        }
			        else
			        {
			            if (!config.UseWebhookLogging)
			            {
				            await channel.SendMessageAsync(m => m.AddEmbed(embed).AddEmbed(contentBefore).AddEmbed(contentAfter));
			            }
			            else
			            {
				            var wh = _whClient.GetRegisteredWebhook(config.WebhookLoggingId) ?? await _whClient.AddWebhookAsync(new(config.LoggingWebhookUrl!));
				            await wh.ExecuteAsync(new DiscordWebhookBuilder().WithAvatarUrl(c.CurrentUser.AvatarUrl).AddEmbed(embed).AddEmbed(contentBefore).AddEmbed(contentAfter));
			            }

			            //TODO: Metrics
			        }
			    }
			    else
			    {
			        var embed = AddLoggingFields(new())
			            .WithTitle("A message was edited:")
			            .WithDescription($"{Formatter.Bold("Content Before:")}\n{e.MessageBefore.Content}\n\n{Formatter.Bold("Content After:")}\n{e.Message.Content}")
			            .WithColor(DiscordColor.Orange);

			        if (!e.Guild.Channels.TryGetValue(config.LoggingChannel, out var channel))
			        {
			            _logger.LogWarning("Configured logging channel was not present in guild: {Guild}", e.Guild.Id);
			        }
			        else
			        {
			            if (!config.UseWebhookLogging)
			            {
				            try
				            {
					            await channel.SendMessageAsync(m => m.AddEmbed(embed));
				            }
				            catch (UnauthorizedException)
				            {
					            _logger.LogWarning("Log channel exists; permissions were changed. Guild: {Guild}", e.Guild.Id);
				            }
				            catch (NotFoundException)
				            {
					            _logger.LogWarning("Log channel was deleted. Guild: {Guild}", e.Guild.Id);
				            }
			            }
			            else
			            {
				            try
				            {
					            var wh = _whClient.GetRegisteredWebhook(config.WebhookLoggingId) ?? await _whClient.AddWebhookAsync(new(config.LoggingWebhookUrl!));
					            
					            
					            await wh.ExecuteAsync(new DiscordWebhookBuilder().WithAvatarUrl(c.CurrentUser.AvatarUrl).AddEmbed(embed));
				            }
				            catch
				            {
					            _logger.LogWarning("Log channel and/or Log webhook have gone missing. Guild: {Guild}", e.Guild.Id);
				            }
			            }
			        }
			    }
			    
			});

            DiscordEmbedBuilder AddLoggingFields(DiscordEmbedBuilder builder)
	            => builder
		            .WithThumbnail(e.Message.Author?.AvatarUrl ?? string.Empty)
		            .AddField("Channel", e.Channel.IsThread ? e.Channel.Parent.Mention : e.Channel.Mention, true)
		            .AddField("Thread", e.Channel.IsThread ? e.Channel.Mention : "None", true)
		            .AddField("\u200b", "\u200b", true)
		            .AddField("Edited at:", Formatter.Timestamp(DateTime.Now), true)
		            .AddField("Sent at:", Formatter.Timestamp(e.Message.Timestamp), true)
		            .AddField("\u200b", "\u200b", true)
		            .AddField("Message ID:", $"[{e.Message.Id}]({e.Message.JumpLink})", true)
		            .AddField("User ID:", e.Message.Author is null ? "I wasn't around at the time. Sorry!" : $"[{e.Message.Author.Id}]({e.Author.GetUrl()})", true);
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

				DiscordEmbedBuilder editEmbed = GetDeletionEmbed(e);
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
					return;
				}

				if (e.Message.Embeds.Any())
				{
					var builder = new DiscordMessageBuilder();
					var theirEmbeds = e.Message.Embeds.Select(e => new DiscordEmbedBuilder(e));

					foreach (var embed in theirEmbeds)
					{
						if (embed.ImageUrl is not null)
						{
							var split = embed.ImageUrl.Split('.');
							var name = split[^2] + "." + split[^1];

							await GetImageAsync(embed.ImageUrl, name, builder);
						}
					}
					
					builder.AddEmbeds(theirEmbeds.Select(embed => embed.Build()));

					await channel.SendMessageAsync(editEmbed);

					await channel.SendMessageAsync(builder);
					
					return;
				}
				
				
				await channel.SendMessageAsync(editEmbed).ConfigureAwait(false);
			});
		}

		public async Task GetImageAsync(string url, string name, DiscordMessageBuilder builder)
		{
			var ret = await _client.GetAsync(url);

			if (!ret.IsSuccessStatusCode)
				return;

			var str = await ret.Content.ReadAsStreamAsync();

			builder.WithFile(name, str);
		}

		private async Task<Stream?> GetSingleAttatchmentAsync(DiscordMessage message)
		{
			var ret = await _client.GetAsync(message.Attachments.First().Url);

			if (!ret.IsSuccessStatusCode)
				return null;

			return await ret.Content.ReadAsStreamAsync();
		}
		
		
		private DiscordEmbedBuilder GetDeletionEmbed(MessageDeleteEventArgs e)
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