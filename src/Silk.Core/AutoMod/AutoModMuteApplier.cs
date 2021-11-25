using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Silk.Core.Services.Interfaces;
using Silk.Extensions.DSharpPlus;
using Silk.Shared.Constants;

namespace Silk.Core.AutoMod
{
	/// <summary>
	/// An AutoMod feature that automatically re-applies mutes when members rejoin a guild.
	/// </summary>
	public sealed class AutoModMuteApplier
	{
		private readonly IInfractionService _infractions;
		private readonly DiscordClient _client;
		private readonly ILogger<AutoModMuteApplier> _logger;
		public AutoModMuteApplier(IInfractionService infractions, DiscordClient client, ILogger<AutoModMuteApplier> logger)
		{
			_infractions = infractions;
			_client = client;
			_logger = logger;
			_client.GuildMemberAdded += HandleMemberJoinAsync;
		}

		private async Task HandleMemberJoinAsync(DiscordClient client, GuildMemberAddEventArgs memberArgs)
		{
			bool isMuted = await _infractions.IsMutedAsync(memberArgs.Member.Id, memberArgs.Guild.Id);
			ulong member = memberArgs.Member.Id;
			ulong guild = memberArgs.Guild.Id;
			ulong automod = _client.CurrentUser.Id;


			if (isMuted)
			{
				await _infractions.MuteAsync(member, guild, automod, "Re-applied active mute on join.", updateExpiration: false);
				await _infractions.AddNoteAsync(member, guild, automod, $"{StringConstants.AutoModMessagePrefix} Automatically re-applied {memberArgs.Member.ToDiscordName()}'s mute on rejoin.");
			}
		}

	}
}