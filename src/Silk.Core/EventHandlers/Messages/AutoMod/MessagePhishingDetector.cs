using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.Entities;
using Silk.Core.Services;
using Silk.Core.Services.Data;
using Silk.Core.Services.Interfaces;
using Silk.Core.Types;
using Silk.Shared.Constants;

namespace Silk.Core.EventHandlers.Messages.AutoMod
{
	public sealed class MessagePhishingDetector
	{
		private static readonly Regex LinkRegex = new(@"[.]*(?:https?:\/\/(www\.)?)?(?<link>[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6})\b([-a-zA-Z0-9()@:%_\+.~#?&\/\/=]*)");

		private const string Phishing = "Message contained a phishing link.";

		private readonly ConfigService _config;
		private readonly AutoModAntiPhisher _phishing;
		private readonly IInfractionService _infractions;
		private readonly ILogger<MessagePhishingDetector> _logger;
		
		public MessagePhishingDetector(
			ConfigService config, DiscordClient client,
			AutoModAntiPhisher phishing, IInfractionService infractions,
			ILogger<MessagePhishingDetector> logger)
		{
			_logger = logger;
			_phishing = phishing;
			_config = config;
			_infractions = infractions;
			client.MessageCreated += CreatedSignal;
		}
		
		private Task CreatedSignal(DiscordClient sender, MessageCreateEventArgs e)
		{
			Task.Run(async () =>
			{
				try
				{
					await HandleCreatedAsync(e.Message);
				}
				catch(Exception ex)
				{
					_logger.LogCritical(ex, "An exception was thrown while handling phishing {GuildId}, {ChannelId}, {MessageId}, {UserId}", e.Guild?.Id, e.Channel?.Id, e.Message?.Id, e.Author?.Id);
				}
			});
			return Task.CompletedTask;
		}


		private async Task HandleCreatedAsync(DiscordMessage message)
		{
			if (message.Channel.Guild is null)
				return;

			var config = await _config.GetModConfigAsync(message.Channel.Guild.Id);

			if (!config.DetectPhishingLinks)
				return;
			
			var match = LinkRegex.Match(message.Content);

			if (match.Success)
			{
				var link = match.Groups["link"].Value;
				
				if (_phishing.IsBlacklisted(link))
				{
					_logger.LogInformation(EventIds.AutoMod, "Caught link {Link}, Message info: Sent by {Author} in {Guild} in {Channel}", link, message.Author.Id, message.Channel.Guild.Name, message.Channel.Name);
					await HandleLinkAsync(link, message);
				}
			}
		}

		private async Task HandleLinkAsync(string link, DiscordMessage message)
		{
			var config = await _config.GetModConfigAsync(message.Channel.Guild.Id);

			if (config.DeletePhishingLinks)
			{
				try { await message.DeleteAsync(Phishing); }
				catch { }
			}

			if (config.NamedInfractionSteps.TryGetValue(AutoModConstants.PhishingLinkDetected, out var inf))
			{
				var authorId = message.Author.Id;
				var guildId = message.Channel.Guild.Id;
				var memberId = message.Channel.Guild.CurrentMember.Id;
				
				var task = inf.Type switch
				{
					InfractionType.Ban => _infractions.BanAsync(authorId, guildId, memberId, $"Sent phishing link: `{link}`."),
					InfractionType.Kick => _infractions.KickAsync(authorId, guildId, memberId, $"Sent phishing link: `{link}`."),
					InfractionType.Note => _infractions.AddNoteAsync(authorId, guildId, memberId, $"Sent phishing link: `{link}`."),
					_ => Task.FromResult(InfractionResult.FailedGenericRequirementsNotFulfilled)
				};

				await task;
			}
			else
			{
				var guild = message.Channel.Guild;

				if (!guild.Channels.TryGetValue(config.LoggingChannel, out var chn))
				{
					_logger.LogWarning("Cannot log phishing in {Guild}. No configured action or log channel.", message.Channel.Guild.Id);
				}
				else
				{
					var embed = new DiscordEmbedBuilder();

					embed.WithTitle("Phishing link detected!")
						.WithColor(DiscordColor.Rose)
						.WithAuthor(message.Author.Username, null, message.Author.AvatarUrl)
						.AddField("Link:", link, true)
						.AddField("User Id:", message.Author.Id.ToString(), true)
						.AddField("Channel:", message.Channel.Mention, true);

					await chn.SendMessageAsync(embed);
				}
			}
		}
	}
}