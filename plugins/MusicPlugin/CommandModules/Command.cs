using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MusicPlugin.Utilities;
using YoutubeExplode.Videos;
using YumeChan.PluginBase.Tools;

namespace MusicPlugin
{
	public class Command : BaseCommandModule
	{
		private readonly GuildMusicService _music;
		private readonly SilkApiClient _api;
		
		public Command(GuildMusicService music, IConfigProvider<IMusicConfig> config)
		{
			_music = music;
			_api = new(config.Configuration);
		}

		[Command]
		[RequireVC]
		public async Task JoinAsync(CommandContext ctx) => _music.ConnectToGuildAsync(ctx.Member.VoiceState.Channel, ctx.Channel);
	
		[Command]
		[RequireVC]
		[RequireSameVC(true)]
		public async Task PlayAsync(CommandContext ctx)
		{
			var player = _music.GetPlayer(ctx.Guild);

			if (player.CommandChannel != ctx.Channel)
				return;
			
			if (!player.Paused)
				return;

			await player.PlayAsync();

			if (player.Paused)
			{
				await ctx.RespondAsync("There's nothing to play.");
			}
		}

		[Command]
		[RequireVC]
		[Priority(1)]
		public async Task PlayAsync(CommandContext ctx, VideoId video)
		{
			var player = _music.GetPlayer(ctx.Guild);

			if ((player?.CommandChannel ?? ctx.Channel) != ctx.Channel)
				return;

			if (player is null)
				await _music.ConnectToGuildAsync(ctx.Member.VoiceState.Channel, ctx.Channel);
			
			var ret = await _api.GetYouTubeVideoAsync(video, ctx.User.Id);

			if (ret is null)
			{
				await ctx.RespondAsync("Hmm, something went wrong while fetching that.");
				return;
			}

			await _api.AddToGuildQueueAsync(ctx.Guild.Id, ret);

			if (await _api.PeekNextTrackAsync(ctx.Guild.Id) is null)
				await ctx.RespondAsync($"Now playing {ret.Title}");
			else await ctx.RespondAsync("Queued 1 track");

			await player.PlayAsync();
		}
		
	}
}