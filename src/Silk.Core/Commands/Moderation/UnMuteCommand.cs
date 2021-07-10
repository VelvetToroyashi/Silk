using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Data.Models;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities;
using Silk.Core.Utilities.HelpFormatter;

namespace Silk.Core.Commands.Moderation
{
	[Category(Categories.Mod)]
	public class UnMuteCommand : BaseCommandModule
	{
		private readonly IInfractionService _infractions;
		public UnMuteCommand(IInfractionService infractions) => _infractions = infractions;

		[Command]
		[RequireFlag(UserFlag.Staff)]
		[Description("Unmutes a member.")]
		public async Task UnmuteAsync(CommandContext ctx, DiscordUser user, [RemainingText] string reason = "Not Given.")
		{
			
			var res = await _infractions.UnMuteAsync(user.Id, ctx.Guild.Id, ctx.User.Id, reason);
			
			
		} 
	}
}