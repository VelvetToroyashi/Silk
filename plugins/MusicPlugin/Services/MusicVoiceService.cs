using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

namespace MusicPlugin.Services
{
	public class MusicVoiceService
	{
		private readonly MusicQueueService _queue;
		private readonly DiscordClient _client;
		private readonly VoiceNextExtension _vnext;
		public MusicVoiceService(MusicQueueService queue, DiscordClient client)
		{
			_queue = queue;
			_client = client;
			_vnext = _client.GetVoiceNext();
		}

		public async Task<bool> JoinAsync(DiscordChannel voice, DiscordChannel commands)
		{
			_queue.SetCommandChannelForGuild(commands);

			try
			{
				var connection = await voice.ConnectAsync();

				if (voice.Type is ChannelType.Stage)
					try { await voice.UpdateCurrentUserVoiceStateAsync(false, DateTimeOffset.Now); }
					catch { } // This shouldn't throw. //

				_queue.ConnectedTo(connection, voice);

				return true;
			}
			catch
			{
				// Something went wrong. Don't know what. //
				return false;
			}
		}

		public async Task<bool> LeaveAsync(DiscordChannel voice)
		{
			if (_vnext.GetConnection(voice.Guild) is null)
				return false;

			_queue.DisposeGuildQueue(voice.Guild.Id);

			return true;
		}
	}
}