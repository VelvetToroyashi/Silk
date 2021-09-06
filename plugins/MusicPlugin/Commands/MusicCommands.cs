using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MusicPlugin.Plugin;
using MusicPlugin.Services;

namespace MusicPlugin.Commands
{
	public class MusicCommands : BaseCommandModule
	{
		private readonly MusicService _music;
		private readonly MusicConfig _config;
		
		
		public MusicCommands(MusicService music)
		{
			_music = music;

		}

		[Command]
		public async Task Play(CommandContext ctx, string url)
		{
			var player = _music.CreateNewPlayer(ctx.Message);

			var client = new SilkApiClient(null);
			var song = await client.GetYouTubeVideoAsync(url, ctx.User.Id);
			await client.AddToGuildQueueAsync(ctx.Guild.Id, song);
			
			await player.StartAsync();
		}
	}
}