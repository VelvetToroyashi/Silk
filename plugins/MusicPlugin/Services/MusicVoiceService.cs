using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

namespace MusicPlugin.Services
{
	public class MusicVoiceService
	{
		private readonly DiscordClient _client;
		private readonly MusicQueueService _queue;
		
		private readonly VoiceNextExtension _vnext;
		
		public MusicVoiceService(DiscordClient client)
		{
			_client = client;
			_vnext = _client.GetVoiceNext();
		}
		/*
		 * TODO: Skip, Repeat, Play, Pause, etc.
		 */
		
		public async Task<bool> ConnectAsync(DiscordChannel channel)
		{
			if (_vnext.GetConnection(channel.Guild) is VoiceNextConnection conn)
				 conn.Disconnect();

			try
			{
				await _vnext.ConnectAsync(channel);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public bool Disconect(DiscordGuild guild)
		{
			if (_vnext.GetConnection(guild) is VoiceNextConnection conn)
			{
				conn.Disconnect();
				return true;
			}
			
			return false;
		}
	}
}