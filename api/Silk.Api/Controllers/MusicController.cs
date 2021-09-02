using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Silk.Api.Helpers;

namespace Silk.Api.Controllers
{
	[Authorize]
	[ApiController]
	[Route("api/v1/[controller]")]
	public class MusicController : Controller
	{
		[HttpGet]
		[Route("videos")]
		public async Task<IActionResult> GetVideoAsync([FromQuery] string video)
		{
			return this.NotImplemented();
		}
		
		[HttpGet]
		[Route("videos/search")]
		public async Task<IActionResult> SearchVideosAsync([FromQuery] string search)
		{
			return this.NotImplemented();
		}
		
		[HttpGet]
		[Route("playlists")]
		public async Task<IActionResult> GetPlaylistAsync([FromQuery] string playlist)
		{
			return this.NotImplemented();
		}

		[HttpGet]
		[Route("playlists/search")]
		public async Task<IActionResult> SearchAsync([FromQuery] string search)
		{
			return this.NotImplemented();
		}

		[HttpGet]
		[Route("{guild}/queue")]
		public async Task<IActionResult> GetGuildQueueAsync(ulong guildId)
		{
			return this.NotImplemented();
		}

		[HttpPost]
		[Route("{guild}/queue")]
		public async Task<IActionResult> AddToGuildQueueAsync(ulong guildId) /* TODO: MusicResult? */
		{
			return this.NotImplemented();
		}
		
		[HttpGet]
		[Route("{guild}/queue/next")]
		public async Task<IActionResult> PeekNextInGuildQueueAsync(ulong guildId) /* TODO: MusicResult? */
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