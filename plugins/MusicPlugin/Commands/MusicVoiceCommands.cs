using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MusicPlugin.Services;

namespace MusicPlugin.Commands
{
	public class MusicVoiceCommands : BaseCommandModule
	{
		private readonly MusicService _music;
		public MusicVoiceCommands(MusicService music) => _music = music;

		[Command]
		[VCCheck]
		public async Task JoinAsync(CommandContext ctx)
		{
			var voice = ctx.Member.VoiceState.Channel;
			var commands = ctx.Channel;
			var res = await _music.JoinAsync(voice, commands);
			var msg = _music.GetFriendlyResultName(res, voice, commands);

			await ctx.RespondAsync(msg);
		}
		
		[Command]
		[VCCheck(true)]
		public async Task LeaveAsync(CommandContext ctx)
		{
			var res = await _music.LeaveAsync(ctx.Member.Guild);
			var msg = _music.GetFriendlyResultName(res);

			await ctx.RespondAsync(msg);
		}
	}
}