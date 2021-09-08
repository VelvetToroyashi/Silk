using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Emzi0767.Utilities;
using MusicPlugin.Plugin;

namespace MusicPlugin
{
	public sealed class GuildMusicPlayer
	{
		public bool Paused => !_pause.IsSet;

		private Stream _current;
		private int _elapsedSeconds;
		private TimeSpan _duration;
		
		private readonly VoiceNextConnection _conn;
		private readonly DiscordChannel _commandChannel;
		private readonly SilkApiClient _api;

		private readonly AsyncManualResetEvent _pause = new(true);
		
		private CancellationTokenSource _cts = new();
		private CancellationToken _token => _cts.Token;
		
		private Process _ffmpeg;
		private readonly ProcessStartInfo _ffmpegInfo = new()
		{
			Arguments = "-hide_banner -loglevel quiet -i - -map 0:a -ac 2 -f s16le -ar 48k pipe:1 ",
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			FileName = "./ffmpeg",
			CreateNoWindow = true,
			UseShellExecute = false,
		};
		
		public GuildMusicPlayer(VoiceNextConnection conn, DiscordChannel commandChannel, SilkApiClient api)
		{
			_conn = conn;
			_commandChannel = commandChannel;
			_api = api;
		}

		public async Task PlayAsync()
		{
			_ffmpeg ??= Process.Start(_ffmpegInfo);
			var sink = _conn.GetTransmitSink();

			await _pause.SetAsync();

			var resuming = true;
			
			if (_current is null)
			{
				resuming = false;
				if (!await GetQueuedSongAsync()) return;
			}

			_ = Task.Run(async () =>
			{
				var httpStreamTask = _current.CopyToAsync(_ffmpeg.StandardInput.BaseStream, _token);
				var vnextTask = _ffmpeg.StandardOutput.BaseStream.CopyToAsync(sink, cancellationToken: _token);
				
				if (!resuming)
					AutoPlayLoopAsync(); // Start the timer //
				
				try { await Task.WhenAll(httpStreamTask, vnextTask); }
				catch { /* The above will throw; it just means we paused/skipped. */ }
			}, CancellationToken.None);
		}

		private async Task AutoPlayLoopAsync()
		{
			await Task.Yield();
			var current = _current;
			while (true)
			{
				if (current != _current)
					return; // Skipped //
				
				await Task.Delay(1000);
				await _pause.WaitAsync();
				
				if (_elapsedSeconds++ >= _duration.TotalSeconds)
				{
					_current = null;
					Stop();
					await PlayAsync();
					return;
				}
			}
		}
		
		public void Pause()
		{
			_pause.Reset();
			
			_cts.Cancel();
			_cts = new();
		}

		private void Stop()
		{
			Pause();

			_elapsedSeconds = 0;
			_ffmpeg?.Kill();
			_ffmpeg?.Dispose();
			_ffmpeg = null;
		}


		private async Task<bool> GetQueuedSongAsync()
		{
			var current = await _api.PeekNextTrackAsync(_commandChannel.Guild.Id) ?? await _api.GetCurrentTrackAsync(_commandChannel.Guild.Id);
				
			if (current is null) return false; // Empty queue //
				
			_current = new HttpStream(_api, current.Url, await _api.GetContentLength(current.Url), null);
			_duration = current.Duration;

			await _api.GetNextTrackAsync(_commandChannel.Guild.Id);

			return true;
		}
	}
}