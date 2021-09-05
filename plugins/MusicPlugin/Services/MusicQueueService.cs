using DSharpPlus.Entities;

namespace MusicPlugin.Services
{
	public sealed class MusicQueueService
	{
		public bool PlayingInGuild(ulong guildId) => false;
		public DiscordChannel GetBoundChannelForGuild(ulong guildId) => null;
	}
}