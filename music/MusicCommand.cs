using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dasync.Collections;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Silk.Core.Types;
using Silk.Core.Utilities.Bot;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Core.Utilities.HttpClient;
using Silk.Extensions.DSharpPlus;
using YoutubeExplode;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace Silk.Core.Services.Bot.Music
{
	[RequireGuild]
	[RequireMusicGuild]
	[HelpCategory(Categories.Music)]
	public class MusicCommand : BaseCommandModule
	{
		private readonly MusicVoiceService _music;
		private readonly YoutubeClient _ytClient;
		private readonly IHttpClientFactory _htClientFactory;
		
		public MusicCommand(MusicVoiceService music, YoutubeClient ytClient, IHttpClientFactory htClientFactory)
		{
			_music = music;
			_ytClient = ytClient;
			_htClientFactory = htClientFactory;
		}

		[Command]
		[Aliases("np")]
		public async Task NowPlaying(CommandContext ctx)
		{
			var np = _music.GetNowPlaying(ctx.Guild.Id);
			if (np is null)
			{
				await ctx.RespondAsync("There's nothing playing.");
			}
			else
			{
				await ctx.RespondAsync($"**{np.Title}** - Requested by {ctx.User.Username} \nDuration: {np.Duration:c}, ends at {Formatter.Timestamp(_music.GetRemainingTime(ctx.Guild.Id), TimestampFormat.LongTime)}.");
			}
		}
		
		
		[Command]
		[RequrieVC]
		[Priority(0)]
		[Aliases("p")]
		public async Task Play(CommandContext ctx)
		{
			var result = await _music.PlayAsync(ctx.Guild.Id);

			if (result is MusicPlayResult.InvalidChannel) 
				await ctx.RespondAsync("I'm not in a channel!");
		}
		
		[Command]
		[RequrieVC]
		[Priority(2)]
		public async Task Play(CommandContext ctx, Video video)
		{
			string message;

			if (ctx.Member.VoiceState.Channel != ctx.Guild.CurrentMember.VoiceState?.Channel && (ctx.Guild.CurrentMember.VoiceState?.Channel?.Users.Count() ?? 0) > 1)
			{
				await ctx.RespondAsync("I'm already in a VC. Sorry!");
				return;
			}
			
			VoiceResult res = await _music.JoinAsync(ctx.Member.VoiceState.Channel, ctx.Channel);
			if (ctx.Guild.CurrentMember.VoiceState?.Channel is not null)
			{
				message = res switch
				{
					VoiceResult.Succeeded => $"Now connected to {ctx.Member.VoiceState.Channel.Mention}!",
					VoiceResult.SameChannel => "",
					VoiceResult.CannotUnsupress => "I'm not a stage moderator! Would you mind accepting my request to speak?",
					VoiceResult.CouldNotJoinChannel => "Awh. I can't join that channel!",
					VoiceResult.NonVoiceBasedChannel => "You...Don't seemt o be in a voice-based channel??",
				};
				
				if (res is not VoiceResult.SameChannel)
					await ctx.RespondAsync(message);
			}

			_music.Enqueue(ctx.Guild.Id, GetTrackAsync);
			
			if (res is (VoiceResult.CouldNotJoinChannel or VoiceResult.NonVoiceBasedChannel or VoiceResult.CannotUnsupress))
				return;
			
			var result = await _music.PlayAsync(ctx.Guild.Id);

			message = result switch
			{
				MusicPlayResult.NowPlaying => $"Now playing {_music.GetNowPlaying(ctx.Guild.Id)?.Title}!",
				MusicPlayResult.AlreadyPlaying => "Queued 1 song.",
				_ => $"Unexpected response {result}"
			};

			await ctx.RespondAsync(message);

			async Task<MusicTrack?> GetTrackAsync()
			{
				try
				{
					var manifest = await _ytClient.Videos.Streams.GetManifestAsync(video.Url);
					var audio = manifest.GetAudioOnlyStreams().FirstOrDefault() ?? manifest.Streams.FirstOrDefault();
					
					if (audio is null)
						return null;
					
					var stream = new LazyLoadHttpStream(_htClientFactory.CreateSilkClient(), audio.Url, audio.Size.Bytes, !Regex.IsMatch(audio.Url, "ratebypass[=/]yes") ? 9_898_989 : null);
					var vid = video;

					return new()
					{
						Title = vid.Title,
						Url = vid.Url,
						Stream = stream,
						Requester = ctx.User,
						Duration = vid.Duration.Value,
					};
				}
				catch { return null; }
			}
		}
	
		[Command]
		[RequrieVC]
		[Priority(1)]
		public async Task Play(CommandContext ctx, IReadOnlyList<PlaylistVideo> playlist)
		{
			string message;

			if (ctx.Member.VoiceState.Channel != ctx.Guild.CurrentMember.VoiceState?.Channel && (ctx.Guild.CurrentMember.VoiceState?.Channel?.Users.Count() ?? 0) > 1)
			{
				await ctx.RespondAsync("I'm already in a VC. Sorry!");
				return;
			}
			
			VoiceResult res = await _music.JoinAsync(ctx.Member.VoiceState.Channel, ctx.Channel);
			if (ctx.Guild.CurrentMember.VoiceState?.Channel is not null)
			{
				message = res switch
				{
					VoiceResult.Succeeded => $"Now connected to {ctx.Member.VoiceState.Channel.Mention}!",
					VoiceResult.SameChannel => "",
					VoiceResult.CannotUnsupress => "I'm not a stage moderator! Would you mind accepting my request to speak?",
					VoiceResult.CouldNotJoinChannel => "Awh. I can't join that channel!",
					VoiceResult.NonVoiceBasedChannel => "You...Don't seemt o be in a voice-based channel??",
				};
				
				if (res is not VoiceResult.SameChannel)
					await ctx.RespondAsync(message);
			}
			
			if (res is (VoiceResult.CouldNotJoinChannel or VoiceResult.NonVoiceBasedChannel or VoiceResult.CannotUnsupress))
				return;
			
			var videos = playlist.Where(p => (p.Duration ?? TimeSpan.MaxValue) < TimeSpan.FromHours(4));
			
			foreach (PlaylistVideo video in videos) 
				_music.Enqueue(ctx.Guild.Id, () => GetTrackAsync(video));


			var result = await _music.PlayAsync(ctx.Guild.Id);

			message = result switch
			{
				MusicPlayResult.NowPlaying => $"Now playing {_music.GetNowPlaying(ctx.Guild.Id)?.Title}!",
				MusicPlayResult.AlreadyPlaying => $"Queued {videos.Count()} video(s).",
				_ => $"Unexpected response {result}"
			};

			await ctx.RespondAsync(message);
			
			async Task<MusicTrack?> GetTrackAsync(PlaylistVideo video)
			{
				try
				{
					var manifest = await _ytClient.Videos.Streams.GetManifestAsync(video.Url);
					var audio = manifest.GetAudioOnlyStreams().First();
					var stream = new LazyLoadHttpStream(_htClientFactory.CreateSilkClient(), audio.Url, audio.Size.Bytes, !Regex.IsMatch(audio.Url, "ratebypass[=/]yes") ? 9_898_989 : null);
					var vid = video;

					return new()
					{
						Title = vid.Title,
						Url = vid.Url,
						Stream = stream,
						Requester = ctx.User,
						Duration = vid.Duration.Value,
					};
				}
				catch { return null; }
			}
		}
		
		[Command]
		[RequrieVC]
		[RequrieSameVC]
		public async Task Pause(CommandContext ctx) => _music.Pause(ctx.Guild.Id);

		[Command]
		[Aliases("q")]
		public async Task Queue(CommandContext ctx)
		{
			if (ctx.Guild.CurrentMember.VoiceState?.Channel is null)
			{
				await ctx.RespondAsync("I'm not even in a voice channel!");
				return;
			}
			
			var results = await _music.GetQueuedTracksAsync(ctx.Guild.Id).ToListAsync();

			if (!results.Any())
			{
				await ctx.RespondAsync("There's nothing in the queue.");
			}
			else
			{
				var embed = new DiscordEmbedBuilder().WithTitle($"Queue for {ctx.Guild.Name}!");
				var sbuilder = new StringBuilder();
				
				var truncatedResults = results.Take(10).ToList();

				var nowPlaying = _music.GetNowPlaying(ctx.Guild.Id)!;
				sbuilder.AppendLine($"⬇ **Now Playing** ⬇\n\n**{nowPlaying.Title}**\n​\t{nowPlaying.Duration:c} - Requested by {nowPlaying.Requester.Mention}\n");
				
				foreach (var result in truncatedResults)
					sbuilder.AppendLine($"{result.Title}\n​\t{result.Duration:c} - Requested by {result.Requester.Mention}\n");

				if (results.Count > truncatedResults.Count)
					sbuilder.AppendLine($"Plus {results.Count + truncatedResults.Count} more.");

				embed.WithDescription(sbuilder.ToString());
				embed.WithColor(DiscordColor.Azure);
				embed.WithAuthor(ctx.User.Username, ctx.User.GetUrl(), ctx.User.AvatarUrl);

				await ctx.RespondAsync(embed);
			}
		}
		
		[Command]
		[RequrieVC]
		[RequrieSameVC]
		public Task Resume(CommandContext ctx) => _music.ResumeAsync(ctx.Guild.Id).AsTask();

		[Command]
		[RequrieVC]
		[Aliases("s")]
		[RequrieSameVC]
		public async Task Skip(CommandContext ctx)
		{
			var vstate = ctx.Member.VoiceState.Channel;
			var vstateUsers = vstate.Users.Count();
			if (vstateUsers < 4) // The bot + 2 others. //
			{
				await _music.SkipAsync(ctx.Guild.Id);
			}
			else
			{
				var interactivity = ctx.Client.GetInteractivity();
				var requiredVotes = vstateUsers * (2 / 3);
				var skip = await ctx.RespondAsync($"**Skip?** (Requires {vstateUsers - 1}/{vstateUsers} votes).");
				await skip.CreateReactionAsync(DiscordEmoji.FromUnicode("⏭"));

				var now = DateTime.UtcNow;
				var expiry = now + TimeSpan.FromSeconds(15);
				var votes = 0;

				while (now > expiry && votes >= requiredVotes)
				{
					var vote = await interactivity.WaitForReactionAsync(m => ((DiscordMember) m.User).VoiceState?.Channel == vstate, now - expiry);
					if (!vote.TimedOut)
						votes++;
				}

				if (votes >= requiredVotes)
					await _music.SkipAsync(ctx.Guild.Id);
			}
		}
	}
}