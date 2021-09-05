using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MusicPlugin.Services;

namespace MusicPlugin.Commands
{
	[RequireGuild]
	public sealed class MusicCommandModule : BaseCommandModule
	{
		private readonly MusicQueueService _music;
		
		public async Task Play(CommandContext ctx, string url)
		{
			
		}


		private async Task<bool> ValidateMemberStateAsync(CommandContext ctx)
		{
			if (ctx.Member.VoiceState?.Channel != ctx.Channel)
				return false;

			return true;
			/* TODO: Send error message before returning. */
		}
	}
}