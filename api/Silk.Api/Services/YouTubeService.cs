using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Silk.Api.Models;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace Silk.Api.Services
{
	public sealed class YouTubeService
	{
		private readonly YoutubeClient _client;
		public YouTubeService(YoutubeClient client) => _client = client;

		public async Task<ApiMusicModel> GetVideoAsync(string video, ulong requester)
		{
			try
			{
				Video metadata = await _client.Videos.GetAsync(video);

				IStreamInfo stream = (await _client.Videos.Streams.GetManifestAsync(video)).GetAudioOnlyStreams().GetWithHighestBitrate();

				return new()
				{
					Url = stream.Url,
					Title = metadata.Title,
					Duration = metadata.Duration ?? TimeSpan.Zero,
					Requester = requester
				};
			}
			catch
			{
				return null;
			}
		}

		public async Task<IEnumerable<ApiMusicModel>> SearchVideosAsync(string query, ulong requester)
		{
			var videos = await _client.Search.GetResultsAsync(query);
			return videos
				.Where(sr => sr is not ChannelSearchResult)
				.Select(sr => new ApiMusicModel
				{
					Url = sr.Url,
					Title = sr.Url,
					Requester = requester,
					Duration = (sr as VideoSearchResult)?.Duration ?? TimeSpan.Zero,
				});
		}
	}
}