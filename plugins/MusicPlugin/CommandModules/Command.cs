using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MusicPlugin.Utilities;

namespace MusicPlugin
{
	public class Command : BaseCommandModule
	{
		private readonly GuildMusicService _music;
		public Command(GuildMusicService music) => _music = music;


		[Command]
		[RequireVC]
		public async Task JoinAsync(CommandContext ctx) => _music.ConnectToGuildAsync(ctx.Member.VoiceState.Channel, ctx.Channel);


	}
}