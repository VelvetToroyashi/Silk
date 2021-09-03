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
			return this.NotImplemented();
		}
		
		[HttpGet]
		[Route("{guildId}/queue/current")]
		public async Task<IActionResult> GetCurrentGuildTrackAsync(ulong guildId)
		{
			return this.NotImplemented();
		}

		[HttpPost]
		[Route("{guildId}/queue")]
		public async Task<IActionResult> AddToGuildQueueAsync(ulong guildId, [FromBody] ApiMusicModel? track = null) /* TODO: MusicResult ? */
		{
			var user = User.FindFirst("ist")!.Value;

			if (!_queue.CreateGuildQueueAsync(user, guildId))
				return Conflict(new { message = "there is already a queue for this guild."});

		return Created(nameof(GetGuildQueueAsync), null);
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
			return this.NotImplemented();
		}

		[HttpPost]
		[Route("{guildId}/queue/shuffle")]
		public async Task<IActionResult> SetGuildQueueShuffleAsync(ulong guildId, [FromBody] bool shuffle)
		{
			return this.NotImplemented();
		}

		[HttpDelete]
		[Route("{guildId}/queue")]
		public async Task<IActionResult> RemoveGuildQueueAsync(ulong guildId)
		{
			var user = User.FindFirst("ist")!.Value;
			
			if (!_queue.RemoveQueueForGuild(user, guildId))
				return NotFound();

			return NoContent(); 
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