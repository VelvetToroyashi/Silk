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
		public async Task PlayAsync(CommandContext ctx)
		{
			if (ctx.Guild.CurrentMember.VoiceState?.Channel is null)
			{
				await ctx.RespondAsync("I need to be in a VC for that..");
				return;
			}
			
			var player = _music.GetPlayer(ctx.Guild);

			if (player?.CommandChannel != ctx.Channel)
				return;
			
			if (!player?.Paused ?? true)
				return;

			await player.PlayAsync();

			if (player.Paused) 
				await ctx.RespondAsync("There's nothing to play.");
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
			{
				await _music.ConnectToGuildAsync(ctx.Member.VoiceState.Channel, ctx.Channel);
				player = _music.GetPlayer(ctx.Guild);
			}
				
			var ret = await _api.GetYouTubeVideoAsync(video, ctx.User.Id);

			if (ret is null)
			{
				await ctx.RespondAsync("Hmm, something went wrong while fetching that.");
				return;
			}

			await _api.AddToGuildQueueAsync(ctx.Guild.Id, ret);

			await ctx.RespondAsync(player.NowPlaying is null ? $"Now playing {ret.Title}" : "Queued 1 track");

			await player.PlayAsync();
		}
	}
}