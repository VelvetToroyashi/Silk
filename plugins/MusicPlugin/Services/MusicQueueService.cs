using System;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

namespace MusicPlugin.Services
{
	public sealed class MusicQueueService
	{
		private sealed class GuildMusic : IDisposable
		{
			public bool Playing { get; set; }
			public DiscordChannel CommandChannel { get; set; }
			public VoiceNextExtension VNextExtension { get; set; }
			public VoiceNextConnection VNextConnection { get; set; }

			~GuildMusic() => Dispose();

			private void Dispose(bool disposing)
			{
				VNextConnection?.Dispose();
			}
			
			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
		}
		
		private readonly Dictionary<ulong, GuildMusic> _guilds = new();

		private readonly DiscordClient _client;
		public MusicQueueService(DiscordClient client) => _client = client;

		public bool PlayingInGuild(ulong guildId) => _guilds.TryGetValue(guildId, out var music) && music.Playing;
		public DiscordChannel GetBoundChannelForGuild(ulong guildId) => _guilds.TryGetValue(guildId, out var music) ? music.CommandChannel : null;

		public void SetCommandChannelForGuild(DiscordChannel channel) => _guilds[channel.Guild.Id] = new() { CommandChannel = channel };

		public void ConnectedTo(VoiceNextConnection connection, DiscordChannel channel)
		{
			if (!_guilds.TryGetValue(channel.Guild.Id, out var music))
				return;

			music.VNextConnection = connection;
			music.VNextExtension = _client.GetVoiceNext();
		}

		public void DisposeGuildQueue(ulong guildId)
		{
			_guilds.Remove(guildId, out var music);
			music?.Dispose();
		}
	}
}