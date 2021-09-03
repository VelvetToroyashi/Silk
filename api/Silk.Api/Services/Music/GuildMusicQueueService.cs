using System.Collections.Generic;
using System.Text.Json.Serialization;
using Silk.Api.Models;

namespace Silk.Api.Services
{
	public record GuildMusicQueue(int CurrentTrackIndex, [property: JsonIgnore] bool Repeat, List<ApiMusicModel> Tracks);
	
	public class GuildMusicQueueService
	{
		private readonly Dictionary<string, Dictionary<ulong, GuildMusicQueue>> _queues = new();

		public void GetGuildQueue(string user, ulong guild, out GuildMusicQueue queue)
		{
			queue = null;

			if (!_queues.TryGetValue(user, out var queues))
				return;

			queues.TryGetValue(guild, out queue);
		}
		
		public bool ClearQueueForGuild(string user, ulong guild)
		{
			if (!_queues.TryGetValue(user, out var queues))
				return false;

			if (!queues.TryGetValue(guild, out var guildQueue))
				return false;
			
			guildQueue.Tracks.Clear();

			return true;
		}
		
		public bool RemoveQueueForGuild(string user, ulong guild)
		{
			if (!_queues.TryGetValue(user, out var queues))
				return false;

			return queues.Remove(guild);
		}
	}
}