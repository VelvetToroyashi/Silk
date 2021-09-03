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
		public MusicController(YouTubeService youtube) => _youtube = youtube;
		
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
		[Route("{guild}/queue")]
		public async Task<IActionResult> GetGuildQueueAsync(ulong guildId, ApiMusicModel track)
		{
			return this.NotImplemented();
		}

		[HttpPost]
		[Route("{guild}/queue")]
		public async Task<IActionResult> AddToGuildQueueAsync(ulong guildId) /* TODO: MusicResult ? */
		{
			return this.NotImplemented();
		}

		[HttpPost]
		[Route("{guild}/queue/bulk")]
		public async Task<IActionResult> AddPlaylistToGuildQueueAsync(ulong guildId) /* TODO: IEnumerable<MusicResult> ? */
		{
			return this.NotImplemented();
		}
		
		[HttpGet]
		[Route("{guild}/queue/next")]
		public async Task<IActionResult> PeekNextInGuildQueueAsync(ulong guildId)
		{
			return this.NotImplemented();
		}
		
		[HttpPost]
		[Route("{guild}/queue/next")]
		public async Task<IActionResult> RequestNextInGuildQueueAsync(ulong guildId) /* TODO: MusicResult? */
		{
			return this.NotImplemented();
		}

		[HttpPost]
		[Route("{guild}/queue/shuffle")]
		public async Task<IActionResult> SetGuildQueueShuffleAsync(ulong guildId, [FromBody] bool shuffle)
		{
			return this.NotImplemented();
		}

		[HttpDelete]
		[Route("{guild}/queue")]
		public async Task<IActionResult> RemoveGuildQueueAsync(ulong guildId)
		{
			return this.NotImplemented();
		}

		[HttpPut]
		[Route("{guild}/queue")]
		public async Task<IActionResult> OverwriteGuildQueueAsync(ulong guildId) /* TODO: IEnumerable<MusicResult> ? */
		{
			return this.NotImplemented();
		}
	}
}