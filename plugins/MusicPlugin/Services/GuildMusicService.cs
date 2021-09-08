using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
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
		}


		public async Task<VoiceJoinState> ConnectToGuildAsync(DiscordChannel voiceChannel, DiscordChannel commandChannel)
		{
			VoiceNextConnection conn = null;

			try
			{
				if (!_players.TryGetValue(commandChannel.Guild.Id, out var player))
				{
					conn = await voiceChannel.ConnectAsync();
					_players[commandChannel.Guild.Id] = new(conn, commandChannel, new(_config));
				}
				else
				{
					if (commandChannel != player.CommandChannel)
						conn.Disconnect();

					conn = await voiceChannel.ConnectAsync();
				}
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