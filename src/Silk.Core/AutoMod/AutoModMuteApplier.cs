using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Silk.Core.Services.Interfaces;

namespace Silk.Core.AutoMod
{
	/// <summary>
	/// An AutoMod feature that automatically re-applies mutes when members rejoin a guild.
	/// </summary>
	public sealed class AutoModMuteApplier
	{
		private readonly IInfractionService _infractions;
		private readonly DiscordShardedClient _client;
		private readonly ILogger<AutoModMuteApplier> _logger;
		public AutoModMuteApplier(IInfractionService infractions, DiscordShardedClient client, ILogger<AutoModMuteApplier> logger)
		{
			_infractions = infractions;
			_client = client;
			_logger = logger;
			_client.GuildMemberAdded += HandleMemberJoinAsync;
		}

		private async Task HandleMemberJoinAsync(DiscordClient client, GuildMemberAddEventArgs memberArgs)
		{
			var isMuted = await _infractions.IsMutedAsync(memberArgs.Member.Id, memberArgs.Guild.Id);
			_logger.LogInformation("Member is muted: {Muted}", isMuted);
			if (isMuted)
			{
				await _infractions.MuteAsync(memberArgs.Member.Id, memberArgs.Guild.Id, _client.CurrentUser.Id, "Re-applied active mute on join.", updateExpiration: false);
			}
		}
		
	}
}