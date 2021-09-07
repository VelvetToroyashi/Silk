using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MusicPlugin
{
	public class SilkApiClient : HttpClient
	{
		private readonly MusicConfig _config;
		public SilkApiClient(MusicConfig config)
		{
			_config = config;
			this.BaseAddress = new(_config?.MusicApiUrl ?? "https://localhost:5001/api/v1");
		}

		public async Task<long> GetContentLength(string url)
		{
			var headers = await this.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
			var length = headers.Content.Headers.ContentLength;
			return length.Value;
		}

		public async Task<MusicApiResponse> GetYouTubeVideoAsync(string video, ulong requester)
		{
			using var req = PrepareMessage(HttpMethod.Get, $"/music/youtube/videos?video={video}&requester={requester}");

			using var res = await this.SendAsync(req);

			var ret = JsonConvert.DeserializeObject<MusicApiResponse>(await res.Content.ReadAsStringAsync());
			
			return ret;
		}

		public async Task<MusicApiResponse[]> GetYouTubePlaylistAsync(string playlist, ulong requester)
		{
			using var req = PrepareMessage(HttpMethod.Get, $"/youtube/playlists?playlist={playlist}&requester={requester}");

			using var res = await this.SendAsync(req);

			var ret = JsonConvert.DeserializeObject<MusicApiResponse[]>(await res.Content.ReadAsStringAsync());
			
			return ret;
		}

		public async Task<MusicApiResponse[]> SearchYouTubeAsync(string query, ulong requester)
		{
			using var req = PrepareMessage(HttpMethod.Get, $"/youtube/videos?search={query}&requester={requester}");

			using var res = await this.SendAsync(req);

			var ret = JsonConvert.DeserializeObject<MusicApiResponse[]>(await res.Content.ReadAsStringAsync());
			
			return ret;
		}
		
		/*
		 * TODO: Spotify/track
		 * TODO: Spotify/playlists
		 * TODO: Spotify/search
		 */

		public async Task<MusicApiResponse> GetGuildQueueAsync(ulong guildId)
		{
			using var req = PrepareMessage(HttpMethod.Get, $"/music/{guildId}/queue");

			using var res = await this.SendAsync(req);

			res.EnsureSuccessStatusCode();
			
			var ret = JsonConvert.DeserializeObject<MusicApiResponse>(await res.Content.ReadAsStringAsync());
			
			return ret;
		}

		public async Task AddToGuildQueueAsync(ulong guildId, MusicApiResponse song)
		{
			using var req = PrepareMessage(HttpMethod.Post, $"/music/{guildId}/queue");

			req.Content = new StringContent(JsonConvert.SerializeObject(song));
			req.Content.Headers.ContentType = new("application/json");
			
			Console.WriteLine($"Client: {BaseAddress} | Request: {req.RequestUri}");
			using var res = await this.SendAsync(req);
			
			res.EnsureSuccessStatusCode();
		}

		public async Task BulkAddToGuildQueueAsync(ulong guildId, IEnumerable<MusicApiResponse> songs)
		{
			using var req = PrepareMessage(HttpMethod.Put, $"/music/{guildId}/queue");

			req.Content = new StringContent(JsonConvert.SerializeObject(songs));

			using var res = await this.SendAsync(req);

			res.EnsureSuccessStatusCode();
		}

		public Task ClearGuildQueueAsync(ulong guildId)
			=> DeleteGuildQueueAsync(guildId, true);

		public Task RemoveGuildQueueAsync(ulong guildId)
			=> DeleteGuildQueueAsync(guildId, false);
		
		private async Task DeleteGuildQueueAsync(ulong guildId, bool clearing)
		{
			using var req = PrepareMessage(HttpMethod.Delete, $"/music/{guildId}/queue?clear={clearing}");

			using var res = await this.SendAsync(req);
		}

		public async Task<MusicApiResponse> PeekNextTrackAsync(ulong guildId)
		{
			using var req = PrepareMessage(HttpMethod.Get, $"/music/{guildId}/queue/next");

			using var res = await this.SendAsync(req);

			res.EnsureSuccessStatusCode();
			
			var ret = JsonConvert.DeserializeObject<MusicApiResponse>(await res.Content.ReadAsStringAsync());
			
			return ret;
		}

		public async Task<MusicApiResponse> GetNextTrackAsync(ulong guildId)
		{
			using var req = PrepareMessage(HttpMethod.Post, $"/music/{guildId}/queue/next");

			using var res = await this.SendAsync(req);

			res.EnsureSuccessStatusCode();
			
			var ret = JsonConvert.DeserializeObject<MusicApiResponse>(await res.Content.ReadAsStringAsync());
			
			return ret;
		}

		public async Task ShuffleQueueAsync(ulong guildId, bool shuffle)
		{
			using var req = PrepareMessage(HttpMethod.Post, $"/music/{guildId}/queue/shuffle");

			req.Content = new StringContent(shuffle.ToString());
			
			using var res = await this.SendAsync(req);

			res.EnsureSuccessStatusCode();
		}
		
		public async Task<MusicApiResponse> GetCurrentTrackAsync(ulong guildId)
		{
			using var req = PrepareMessage(HttpMethod.Get, $"/music/{guildId}/queue/current");

			using var res = await this.SendAsync(req);

			res.EnsureSuccessStatusCode();
			
			var ret = JsonConvert.DeserializeObject<MusicApiResponse>(await res.Content.ReadAsStringAsync());
			
			return ret;
		}


		private HttpRequestMessage PrepareMessage(HttpMethod method, string path)
		{
			return new(method, BaseAddress + path)
			{
				Headers =
				{
					Authorization = new("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3QiOiI3NDUzMjUyNjg2NDU3MDc5MTYiLCJpYXQiOiIwOS8wMi8yMDIxIDE5OjQzOjAwIiwiaXNzIjoiaHR0cHM6Ly9hcGkudmVsdmV0dGhlcGFuZGEuZGV2In0.iwbXXtydZv1pp3E49KdtIgD5lpYPGtOQn8MYjkk3yWY"),
				}
				
			};
		}
	}
}