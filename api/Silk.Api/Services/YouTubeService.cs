using System.Threading.Tasks;
using YoutubeExplode;

namespace Silk.Api.Services
{
	public sealed class YouTubeService
	{
		private readonly YoutubeClient _client;
		public YouTubeService(YoutubeClient client) => _client = client;

		public async Task GetVideoAsync(string video)
		{
			var metadata = await _client.Videos.GetAsync(video);
			var stream = await _client.Videos.Streams.GetManifestAsync(video);
			

		}
		
	}
}