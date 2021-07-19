using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
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
		public AutoModMuteApplier(IInfractionService infractions, DiscordShardedClient client)
		{
			_infractions = infractions;
			_client = client;
			_client.GuildMemberAdded += HandleMemberJoinAsync;
		}

		private async Task HandleMemberJoinAsync(DiscordClient client, GuildMemberAddEventArgs memberArgs)
		{
			if (await _infractions.IsMutedAsync(memberArgs.Member.Id, memberArgs.Guild.Id))
				await _infractions.MuteAsync(memberArgs.Member.Id, memberArgs.Guild.Id, _client.CurrentUser.Id, "Re-applied active mute on join.", updateExpiration: false);
		}
		
	}
}