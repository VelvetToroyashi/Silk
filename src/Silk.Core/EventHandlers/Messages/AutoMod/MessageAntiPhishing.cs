using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace Silk.Core.EventHandlers.Messages.AutoMod
{
	public sealed class MessageAntiPhishing
	{
		private static readonly Regex LinkRegex = new(@"^[\s\S]*(?<link>https?://.+\..{2,})");
		
		private readonly ILogger<MessageAntiPhishing> _logger;
		public MessageAntiPhishing(DiscordClient client, ILogger<MessageAntiPhishing> logger)
		{
			_logger = logger;
			client.MessageCreated += CreatedSignal;
		}
		
		private Task CreatedSignal(DiscordClient sender, MessageCreateEventArgs e)
		{
			Task.Run(() => HandleCreatedAsync(e.Message));
			return Task.CompletedTask;
		}


		private async Task HandleCreatedAsync(DiscordMessage message)
		{
			
		}
	}
}