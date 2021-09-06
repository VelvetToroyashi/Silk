using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.Logging;
using MusicPlugin.Models;
using Newtonsoft.Json;
using YumeChan.PluginBase.Tools;

namespace MusicPlugin.Services
{
	public sealed class MusicService
	{


		private readonly Dictionary<ChannelJoinResult, string> _joinResultMessages = new()
		{
			[ChannelJoinResult.AlreadyInChannel] = "I'm already in that channel!",
			[ChannelJoinResult.CannotJoinChannel] = "I don't have permission to join that channel!",
			[ChannelJoinResult.ConnectedToChannel] = "Now connected to {Voice} and bound to {Commands}!",
			[ChannelJoinResult.CannotUnsupressInStage] = "I managed to connect, but I can't unsupress myself!",
			[ChannelJoinResult.DisconnectedFromCurrentVC] = "Alright! Ready to serve if need be, always!",
			[ChannelJoinResult.DisconnectedFromVCAlready] = "Hmm, I don't seem to be in a VC at the moment. Did you mean to ask me to join?", 
		};

		private readonly MusicPluginConfig _config;

		private readonly HttpClient _http;
		private readonly DiscordClient _discord;
		private readonly ILogger<MusicService> _logger;

		private readonly Dictionary<ulong, MusicState> _activeGuilds = new();
		public MusicService(HttpClient http, DiscordClient discord, ILogger<MusicService> logger, IConfigProvider<MusicPluginConfig> configProvider)
		{
			_http = http;
			_discord = discord;
			_logger = logger;

			_config = configProvider.Configuration;
		}

		#region Misc

		public string GetFriendlyResultName(ChannelJoinResult result, DiscordChannel voice = null, DiscordChannel commands = null)
			=> _joinResultMessages[result].Replace("{Voice}", voice?.Mention).Replace("{Commands}", commands?.Mention);

		#endregion
		
		#region Voice

		public bool ConnectedToChannelInGuild(DiscordGuild guild) => _activeGuilds.TryGetValue(guild.Id, out _);

		public async Task<ChannelJoinResult> JoinAsync(DiscordChannel voice, DiscordChannel commands)
		{
			if (!voice.PermissionsFor(voice.Guild.CurrentMember).HasPermission(Permissions.AccessChannels | Permissions.Speak))
				return ChannelJoinResult.CannotJoinChannel;
				
			if (_activeGuilds.TryGetValue(voice.Guild.Id, out var state))
				if (state.Voice == voice)
					return ChannelJoinResult.AlreadyInChannel;
				else
					state.VNext.Disconnect();

			var vne = _discord.GetVoiceNext();
			var vnext = await vne.ConnectAsync(voice);

			_activeGuilds[voice.Guild.Id] = new()
			{
				VNext = vnext,
				Voice = voice,
				Commands = commands
			};

			if (voice.Type is ChannelType.Stage)
			{
				var current = voice.Guild.CurrentMember;
				
				if (!current.VoiceState.IsSuppressed)
					return ChannelJoinResult.ConnectedToChannel;

				try
				{
					await voice.UpdateCurrentUserVoiceStateAsync(false);
					return ChannelJoinResult.ConnectedToChannel;
				}
				catch
				{
					await voice.UpdateCurrentUserVoiceStateAsync(null, DateTimeOffset.Now);
					return ChannelJoinResult.CannotUnsupressInStage;
				}
			}

			return ChannelJoinResult.ConnectedToChannel;
		}

		public async Task<ChannelJoinResult> LeaveAsync(DiscordGuild guild)
		{
			if (!_activeGuilds.Remove(guild.Id, out var state))
				return ChannelJoinResult.DisconnectedFromVCAlready;

			await state.VNext.GetTransmitSink().FlushAsync();
			state.VNext.Dispose();
			
			return ChannelJoinResult.DisconnectedFromCurrentVC;
		}
		
		#endregion
		
		#region Music API
		
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
			_clearQuery = "?clear=",
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

		public async Task<MusicResponseModel> GetYouTubeVideoAsync(DiscordUser requester, string url)
		{
			using var req = PrepareRequest(HttpMethod.Get, _YouTubeVideo + url + _requesterQuery + requester.Id.ToString());

			using var res = await _http.SendAsync(req);

			if (res.StatusCode is HttpStatusCode.NotFound)
				return null;

			res.EnsureSuccessStatusCode();

			var ret = JsonConvert.DeserializeObject<MusicResponseModel>(await res.Content.ReadAsStringAsync());
			
			return ret;
		}
		
		public async Task<MusicResponseModel[]> GetYouTubePlaylistAsync(string url, DiscordUser requester)
		{
			using var req = PrepareRequest(HttpMethod.Get, _YouTubePlaylist + _playlistQuery + url + _requesterQuery + requester);

			using var res = await _http.SendAsync(req);

			var ret = JsonConvert.DeserializeObject<MusicResponseModel[]>(await res.Content.ReadAsStringAsync());
			
			return ret;
		}
		
		private HttpRequestMessage PrepareRequest(HttpMethod method, string endpoint, ulong? guild = null)
		{
			var escaped = new Uri(Uri.EscapeUriString(_config.MusicApiUrl + _url + (guild.HasValue ? $"/{guild.Value}" : null) + endpoint));
			return new(method, escaped)
			{
				Headers =
				{
					Authorization = new("Bearer", _config.ApiKey)
				}
			};
		}
		
		#endregion
		
		
	}
}