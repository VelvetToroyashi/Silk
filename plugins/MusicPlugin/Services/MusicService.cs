using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.Logging;

namespace MusicPlugin.Services
{
	public sealed class MusicService
	{
		private sealed class MusicState
		{
			public DiscordChannel Voice { get; set; }
			public DiscordChannel Commands { get; set; }
			
			public VoiceNextConnection VNext { get; set; }
		}

		public enum ChannelJoinResult
		{
			AlreadyInChannel,
			CannotJoinChannel,
			ConnectedToChannel,
			CannotUnsupressInStage,
			DisconnectedFromCurrentVC,
			DisconnectedFromVCAlready,
			
		}

		private readonly Dictionary<ChannelJoinResult, string> _joinResultMessages = new()
		{
			[ChannelJoinResult.AlreadyInChannel] = "I'm already in that channel!",
			[ChannelJoinResult.CannotJoinChannel] = "I don't have permission to join that channel!",
			[ChannelJoinResult.ConnectedToChannel] = "Now connected to {Voice} and bound to {Commands}",
			[ChannelJoinResult.CannotUnsupressInStage] = "I managed to connect, but I can't unsupress myself!",
			[ChannelJoinResult.DisconnectedFromCurrentVC] = "Alright! Ready to serve if need be, always!",
			[ChannelJoinResult.DisconnectedFromVCAlready] = "Hmm, I don't seem to be in a VC at the moment. Did you mean to ask me to join?", 
		};

		private readonly HttpClient _http;
		private readonly DiscordClient _discord;
		private readonly ILogger<MusicService> _logger;

		private readonly Dictionary<ulong, MusicState> _activeGuilds = new();
		public MusicService(HttpClient http, DiscordClient discord, ILogger<MusicService> logger)
		{
			_http = http;
			_discord = discord;
			_logger = logger;
		}

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
			if (!_activeGuilds.TryGetValue(guild.Id, out var state))
				return ChannelJoinResult.DisconnectedFromVCAlready;

			await state.VNext.GetTransmitSink().FlushAsync();
			state.VNext.Dispose();
			
			return ChannelJoinResult.DisconnectedFromCurrentVC;
		}
		
		#endregion

		#region Music API

		#endregion
	}
}