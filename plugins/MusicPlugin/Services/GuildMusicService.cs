using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using MusicPlugin.Models;
using YumeChan.PluginBase.Tools;

namespace MusicPlugin
{
	public sealed class GuildMusicService
	{
		private readonly Dictionary<ulong, GuildMusicPlayer> _players = new();
		
		private readonly IMusicConfig _config;
		private readonly DiscordClient _client;
		
		public GuildMusicService(IConfigProvider<IMusicConfig> config, DiscordClient client)
		{
			_client = client;
			_config = config.Configuration;
			_client.VoiceStateUpdated += HandleVStateUpdated;
			_client.GuildDeleted += HandleGDeleted;
		}
		
		private async Task HandleGDeleted(DiscordClient sender, GuildDeleteEventArgs e)
		{
		}

		private async Task HandleVStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
		{
			var guildId = e.Guild.Id;

			if (!_players.TryGetValue(guildId, out var player))
			{
				if (e.User == _client.CurrentUser && e.After is null)
				{
					player.Dispose();
					_players.Remove(guildId);
				}
			}
		}
		
		public async Task<VoiceJoinState> ConnectToGuildAsync(DiscordChannel voiceChannel, DiscordChannel commandChannel)
		{
			try
			{
				if (_players.Remove(commandChannel.Guild.Id, out var player))
					player.Dispose();

				VoiceNextConnection conn = await voiceChannel.ConnectAsync();

				_players[commandChannel.Guild.Id] = new(conn, commandChannel, new(_config));
			}
			catch
			{
				return VoiceJoinState.CannotJoinChannel;
			}
			
			if (voiceChannel.Type is ChannelType.Stage)
			{
				try
				{
					await voiceChannel.UpdateCurrentUserVoiceStateAsync(false);
					return VoiceJoinState.Joined;
				}
				
				catch
				{
					if (voiceChannel.Guild.CurrentMember.Permissions.HasPermission(Permissions.RequestToSpeak))
						await voiceChannel.UpdateCurrentUserVoiceStateAsync(null, DateTimeOffset.Now);
					return VoiceJoinState.CannotUnsupress;
				}
			}

			return VoiceJoinState.Joined;
		}
	}
}