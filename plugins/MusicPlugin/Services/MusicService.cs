using System;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;

namespace MusicPlugin.Services
{
	public sealed class MusicService
	{
		private readonly Dictionary<ulong, MusicPlayer> _players = new();

		private readonly MusicConfig _config;
		private readonly DiscordClient _client;
		public MusicService(MusicConfig config, DiscordClient client)
		{
			_config = config;
			_client = client;
		}

		public MusicPlayer CreateNewPlayer(DiscordMessage initiatorMessage)
		{
			var guild = initiatorMessage.Channel.Guild.Id;

			if (_players.ContainsKey(initiatorMessage.Channel.Guild.Id))
				throw new InvalidOperationException("A player already exists for the specified guild");


			return _players[guild] = new(_config, _client, ((DiscordMember)initiatorMessage.Author).VoiceState.Channel, initiatorMessage.Channel); ;
		}

		public MusicPlayer GetMusicPlayer(DiscordGuild guild) => _players.TryGetValue(guild.Id, out var player) ? player : null;
		
		public void RemovePlayer(DiscordGuild guild)
		{
			if (!_players.Remove(guild.Id))
				throw new InvalidOperationException("No player existed for the specified guild.");
		}
	}
}