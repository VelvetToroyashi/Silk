using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using MusicPlugin.Models;
using Newtonsoft.Json;
using YumeChan.PluginBase.Tools;

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
			_videoQuery = "?video=",
			_trackQuery = "?track=",
			_playlistQuery = "?playlist=",
			_searchQuery = "?search=",
			_requesterQuery = "&requester=",
			
			// YouTube (Don't C&D me plz) //
			_YouTube = "/YouTube",
			_YouTubeVideo = _YouTube + _videos ,
			_YouTubeSearch = _YouTube + _search + _searchQuery,
			_YouTubePlaylist = _YouTube + _playlists,
			
			// Spotify //
			_Spotify = "/Spotify",
			_SpotifyTrack = _Spotify + _tracks,
			_SpotifySearch = _Spotify + _search + _searchQuery,
			_SpotifyPlaylist = _Spotify + _playlists;

		private readonly string _apiKey;
		private readonly string _baseUrl;
		private readonly HttpClient _client;
		private readonly ILogger<MusicApiService> _logger;
		
		public MusicApiService(HttpClient client, ILogger<MusicApiService> logger, IConfigProvider<MusicPluginConfig> config)
		{
			_apiKey = config.Configuration.ApiKey;
			_baseUrl = config.Configuration.MusicApiUrl;
			_client = client;
			_logger = logger;
		}
		
		public async Task<MusicResponseModel> GetYouTubeVideoAsync(string url, DiscordUser requester)
		{
			var req = PrepareRequest(HttpMethod.Get, _YouTubeVideo + url + _requesterQuery + requester.Id.ToString());

			var res = await _client.SendAsync(req);

			if (res.StatusCode is HttpStatusCode.NotFound)
				return null;

			res.EnsureSuccessStatusCode();

			var ret = JsonConvert.DeserializeObject<MusicResponseModel>(await res.Content.ReadAsStringAsync());
			
			return ret;
		}

		public async Task<MusicResponseModel[]> GetYouTubePlaylistAsync(string url, DiscordUser requester)
		{
			var req = PrepareRequest(HttpMethod.Get, _YouTubePlaylist + _playlistQuery + url + _requesterQuery + requester);

			var res = await _client.SendAsync(req);

			var ret = JsonConvert.DeserializeObject<MusicResponseModel[]>(await res.Content.ReadAsStringAsync());
			
			return ret;
		}

		private HttpRequestMessage PrepareRequest(HttpMethod method, string endpoint, ulong? guild = null)
		{
			var escaped = new Uri(Uri.EscapeUriString(_baseUrl + (guild.HasValue ? $"/{guild.Value}" : null) + endpoint));
			return new(method, escaped)
			{
				Headers =
				{
					Authorization = new("Bearer", _apiKey)
				}
			};
		}
	}
}