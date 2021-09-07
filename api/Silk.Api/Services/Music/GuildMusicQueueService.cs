using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Silk.Api.Models;

namespace Silk.Api.Services
{
	public record GuildMusicQueue(int CurrentTrackIndex, [property: JsonIgnore] bool Repeat, List<ApiMusicModel> Tracks);
	
	public class GuildMusicQueueService
	{
		private readonly Dictionary<string, Dictionary<ulong, GuildMusicQueue>> _queues = new();
		public bool GetGuildQueue(string user, ulong guild, out GuildMusicQueue queue)
		{
			queue = null;

			if (!_queues.TryGetValue(user, out var queues))
				return false;

			return queues.TryGetValue(guild, out queue);
		}

		public bool CreateGuildQueueAsync(string user, ulong guild)
		{
			if (GetGuildQueue(user, guild, out _))
				return false;

			_queues[user] = new();
			_queues[user][guild] = new(0, false, new());

			return true;
		}

		public bool GetCurrentTrack(string user, ulong guild, out ApiMusicModel audio)
		{
			audio = null;
			
			if (!GetGuildQueue(user, guild, out var queue))
				return false;

			if (queue.Tracks.Count is 0)
				return false;

			audio = queue.Tracks[queue.CurrentTrackIndex];

			return true;
		}
		
		public bool PeekNextTrack(string user, ulong guild, out ApiMusicModel audio)
		{
			audio = null;
			
			if (!GetGuildQueue(user, guild, out var queue))
				return false;

			if (queue.Tracks.Count is 0)
				return false;
			
			if (queue.CurrentTrackIndex + 1 == queue.Tracks.Count)
				if (!queue.Repeat)
					return false;

			audio = queue.Tracks[(queue.CurrentTrackIndex + 1) % queue.Tracks.Count]; // Easy way to return the first element in case of an overflow, I think. //

			return true;
		}
		
		public bool GetNextTrack(string user, ulong guild, out ApiMusicModel audio)
		{
			audio = null;
			
			if (!GetGuildQueue(user, guild, out var queue))
				return false;

			if (queue.Tracks.Count is 0)
				return false;
			
			if (queue.CurrentTrackIndex + 1 == queue.Tracks.Count)
			{
				if (!queue.Repeat)
				{
					return false;
				}
				else
				{
					var tracks = new List<ApiMusicModel>(queue.Tracks.Select(t => t with { Played = false }));
					queue = queue with { CurrentTrackIndex = 0, Tracks = tracks };
				}
			}
			else _queues[user][guild] = queue = queue with { CurrentTrackIndex = queue.CurrentTrackIndex + 1 };

			audio = queue.Tracks[queue.CurrentTrackIndex];

			return true;
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