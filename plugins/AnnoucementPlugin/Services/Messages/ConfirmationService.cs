using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace AnnoucementPlugin.Services
{
	public class ConfirmationService : IConfirmationService
	{
		private readonly DiscordClient _client;
		public ConfirmationService(DiscordClient client) => _client = client;

		public async Task<bool> GetConfirmationAsync(ulong userId, ulong guildId, ulong channelId, string prompt)
		{
			var confirm = new DiscordButtonComponent(ButtonStyle.Success, new Guid().ToString(), "Yes");
			var decline = new DiscordButtonComponent(ButtonStyle.Danger, new Guid().ToString(), "No");
			
			var guild = _client.Guilds[guildId];
			var message = await guild.Channels[channelId].SendMessageAsync(m => m.WithContent(prompt).AddComponents(confirm, decline));

			var result = await message.WaitForButtonAsync(guild.Members[userId], CancellationToken.None);

			return result.Result.Id == confirm.CustomId;
		}
	}
}