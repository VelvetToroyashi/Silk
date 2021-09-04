using System;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using MusicPlugin.Models;

namespace MusicPlugin.Services
{
	public sealed class MusicApiService
	{
		private const string
			_url = "api/v1/music",
			// Endpoints //
			_queue = "/queue",
			_queueBulk = _queue + "/bulk",
			_queueNext = _queue + "/next",
			_queueShuffle = _queue + "/shuffle",
			_queueCurrent = _queue + "/current",
			
			// Helper consts //
			_search = "/search",
			_videos = "/videos",
			_tracks = "/tracks",
			_playlists = "/playlists",
			
			// YouTube (Don't C&D me plz) //
			_YouTube = "/YouTube",
			_YouTubeSearch = _YouTube + _search,
			_YouTubeVideo = _YouTube + _videos,
			_YouTubePlaylist = _YouTube + _playlists,
			// Spotify //
			_Spotify = "/Spotify",
			_SpotifySearch = _Spotify + _search,
			_SpotifyTrack = _Spotify + _tracks,
			_SpotifyPlaylist = _Spotify + _playlists;
		
		private readonly HttpClient _client;
		private readonly ILogger<MusicApiService> _logger;
		
		public MusicApiService(HttpClient client, ILogger<MusicApiService> logger)
		{
			_client = client;
			_logger = logger;
		}

		public async Task<MusicResponseModel> GetYouTubeVideoAsync(string url, DiscordUser requestedBy)
		{
			var query = new Uri("");
			var req = new HttpRequestMessage()
				{ };

			return null;
		}
		
	}
}