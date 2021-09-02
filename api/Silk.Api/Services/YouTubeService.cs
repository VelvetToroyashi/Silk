using YoutubeExplode;

namespace Silk.Api.Services
{
	public sealed class YouTubeService
	{
		private readonly YoutubeClient _client;
		public YouTubeService(YoutubeClient client) => _client = client;

	}
}