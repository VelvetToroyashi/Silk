using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Silk.Core.Services;
using Silk.Shared.Constants;

namespace Silk.Core.EventHandlers.Messages.AutoMod
{
	public sealed class MessagePhishingDetector
	{
		private static readonly Regex LinkRegex = new(@"[.]*(?:https?:\/\/(www\.)?)?(?<link>[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/\/=]*))");
		
		private readonly ILogger<MessagePhishingDetector> _logger;
		private readonly AutoModAntiPhispher _phishing;
		public MessagePhishingDetector(DiscordClient client, ILogger<MessagePhishingDetector> logger, AutoModAntiPhispher phishing)
		{
			_logger = logger;
			_phishing = phishing;
			client.MessageCreated += CreatedSignal;
		}
		
		private Task CreatedSignal(DiscordClient sender, MessageCreateEventArgs e)
		{
			Task.Run(() => HandleCreatedAsync(e.Message));
			return Task.CompletedTask;
		}


		private async Task HandleCreatedAsync(DiscordMessage message)
		{
			if (message.Channel.Guild is null)
				return;
			
			var match = LinkRegex.Match(message.Content);

			if (match.Success)
			{
				var link = match.Groups["link"].Value;
				
				if (_phishing.IsBlacklisted(link))
				{
					_logger.LogInformation(EventIds.AutoMod, "Caught link {Link}, Message info: Sent by {Author} in {Guild} in {Channel} (Thread: {IsThread})", link, message.Author, message.Channel.Guild.Name, message.Channel.Name, message.Channel.IsThread);
					// Todo: Get mod settings?
				}
			}
		}
	}
}