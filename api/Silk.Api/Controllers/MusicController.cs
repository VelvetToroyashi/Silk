using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Silk.Api.Helpers;
using Silk.Api.Models;
using Silk.Api.Services;
using YoutubeExplode.Videos;

namespace Silk.Api.Controllers
{
	[Authorize]
	[ApiController]
	[Route("api/v1/[controller]")]
	public class MusicController : Controller
	{
		private readonly YouTubeService _youtube;
		private readonly GuildMusicQueueService _queue;
		public MusicController(YouTubeService youtube, GuildMusicQueueService queue)
		{
			_youtube = youtube;
			_queue = queue;
		}
		
		[HttpGet]
		[Route("youtube/videos")]
		public async Task<IActionResult> GetVideoAsync([FromQuery] string video, [FromQuery] ulong requester)
		{
			if (VideoId.TryParse(video) is null)
				return BadRequest(new { message = "The provided video id was not valid."});

			var music = await _youtube.GetVideoAsync(video, requester);

			if (music is null) 
				return NotFound();
			
			return Ok(music);
		}
		
		[HttpGet]
		[Route("youtube/playlists")]
		public async Task<IActionResult> GetYouTubePlaylistAsync([FromQuery] string playlist, [FromQuery] ulong requester)
		{
			return this.NotImplemented();
		}

		[HttpGet]
		[Route("youtube/search")]
		public async Task<IActionResult> SearchYouTubeAsync([FromQuery] string search)
		{
			var videos = await _youtube.SearchVideosAsync(search);

			return Ok(videos);
		}

		[HttpGet]
		[Route("spotify/track")]
		public async Task<IActionResult> GetSpotifyTrackAsync(string trackUrl)
		{
			return this.NotImplemented();
		}

		[HttpGet]
		[Route("spotify/playlists")]
		public async Task<IActionResult> GetSpotifyPlaylistAsync(string playlistUrl)
		{
			return this.NotImplemented();
		}
		
		[HttpGet]
		[Route("spotify/search")]
		public async Task<IActionResult> SearchSpotifyAsync([FromQuery] string search)
		{
			return this.NotImplemented();
		}
		
		[HttpGet]
		[Route("{guildId}/queue")]
		public async Task<IActionResult> GetGuildQueueAsync(ulong guildId)
		{
			var user = User.FindFirst("ist").Value;

			if (!_queue.GetGuildQueue(user, guildId, out var queue))
				return NotFound();

			return Ok(queue.Tracks);
		}

		[HttpGet]
		[Route("{guildId}/queue/current")]
		public async Task<IActionResult> GetCurrentGuildTrackAsync(ulong guildId)
		{
			return this.NotImplemented();
		}

		[HttpPost]
		[Route("{guildId}/queue")]
		public IActionResult AddToGuildQueueAsync(ulong guildId, [FromBody] ApiMusicModel track)
		{
			var user = User.FindFirst("ist")!.Value;

			if (!_queue.CreateGuildQueueAsync(user, guildId))
				return Conflict(new { message = "there is already a queue for this guild."});

			if (track is not null)
			{
				_queue.GetGuildQueue(user, guildId, out var queue);
				queue.Tracks.Add(track);
			}
			
			return CreatedAtAction("GetGuildQueue", new { guildId }, null);
		}

		[HttpPost]
		[Route("{guildId}/queue/bulk")]
		public async Task<IActionResult> AddPlaylistToGuildQueueAsync(ulong guildId) /* TODO: IEnumerable<MusicResult> ? */
		{
			return this.NotImplemented();
		}
		
		[HttpGet]
		[Route("{guildId}/queue/next")]
		public async Task<IActionResult> PeekNextInGuildQueueAsync(ulong guildId)
		{
			return this.NotImplemented();
		}
		
		[HttpPost]
		[Route("{guildId}/queue/next")]
		public async Task<IActionResult> RequestNextInGuildQueueAsync(ulong guildId)
		{
			var user = User.FindFirst("ist").Value;

			if (!_queue.GetGuildQueue(user, guildId, out _))
				return NotFound();

			if (!_queue.GetNextTrack(user, guildId, out var nowPlaying))
				return NoContent();

			return Ok(nowPlaying);
		}
		
		[HttpPatch]
		[Route("{guildId}/queue/shuffle")]
		public async Task<IActionResult> SetGuildQueueShuffleAsync(ulong guildId, [FromBody] bool shuffle)
		{
			return this.NotImplemented();
		}

		[HttpDelete]
		[Route("{guildId}/queue")]
		public async Task<IActionResult> RemoveGuildQueueAsync(ulong guildId, [FromQuery] bool clear = false)
		{
			var user = User.FindFirst("ist")!.Value;

			if (!_queue.GetGuildQueue(user, guildId, out _))
				return NotFound();

			if (clear)
				_queue.ClearQueueForGuild(user, guildId);
			else 
				_queue.RemoveQueueForGuild(user, guildId);
			
			
			return clear ? NoContent() : new StatusCodeResult(410); // 410 Gone //
		}

		[HttpPut]
		[Route("{guildId}/queue")]
		public async Task<IActionResult> OverwriteGuildQueueAsync(ulong guildId)
		{
			var user = User.FindFirst("ist")!.Value;
			
			if (!_queue.ClearQueueForGuild(  user, guildId))
				return NotFound();

			return NoContent();
		}
	}
}