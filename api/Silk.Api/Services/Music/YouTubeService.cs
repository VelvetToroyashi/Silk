using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Silk.Api.Models;
using YoutubeExplode;
using YoutubeExplode.Common;
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
			var videos = await _client.Search.GetVideosAsync(query).CollectAsync(15);
			
			return videos
				.Select(sr => new ApiMusicModel
				{
					Url = sr.Url,
					Title = sr.Title,
					Requester = requester,
					Duration = sr.Duration ?? TimeSpan.Zero,
				});
		}
		
		public async Task<IEnumerable<ApiMusicModel>> GetPlaylistAsync(string playlist, ulong requester)
		{
			var videos = await _client.Playlists.GetVideosAsync(playlist);

			return videos.Select(pl => new ApiMusicModel
			{
				Url = pl.Url,
				Title = pl.Title,
				Requester = requester,
				Duration = pl.Duration ?? TimeSpan.Zero,
			});
		}
	}
}