using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

namespace MusicPlugin.Services
{
	public sealed class MusicQueueService
	{
		private sealed class GuildMusic
		{
			public bool Playing { get; set; }
			public DiscordChannel CommandChannel { get; set; }
			public VoiceNextExtension VNextExtension { get; set; }
			public VoiceNextConnection VNextConnection { get; set; }
		}
		
		private readonly Dictionary<ulong, GuildMusic> _guilds = new();

		private readonly DiscordClient _client;
		public MusicQueueService(DiscordClient client) => _client = client;

		public bool PlayingInGuild(ulong guildId) => _guilds.TryGetValue(guildId, out var music) && music.Playing;
		public DiscordChannel GetBoundChannelForGuild(ulong guildId) => _guilds.TryGetValue(guildId, out var music) ? music.CommandChannel : null;
		
		
	}
}