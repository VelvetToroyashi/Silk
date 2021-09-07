using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.VoiceNext;

namespace MusicPlugin
{
	public class Command : BaseCommandModule
	{
		private GuildMusicService _music;
		
		[Command]
		public async Task Play(CommandContext ctx, string url)
		{
			if (_music is not null)
			{
				try { await ctx.Member.VoiceState.Channel.ConnectAsync(); }
				catch { }
				
				await _music.PlayAsync();
				return;
			}
			
			
			var client = new SilkApiClient(null);
			var conn = await ctx.Member.VoiceState.Channel.ConnectAsync();

			_music = new(conn, ctx.Channel, client);

			var track = await client.GetYouTubeVideoAsync(url, ctx.User.Id);
			await client.AddToGuildQueueAsync(ctx.Guild.Id, track);
			
			await _music.PlayAsync();
		}

		[Command]
		public async Task Pause(CommandContext ctx) => _music.Pause();
		
		[Command]
		public async Task Leave(CommandContext ctx) => ctx.Client.GetVoiceNext().GetConnection(ctx.Guild)?.Disconnect();
	}
}