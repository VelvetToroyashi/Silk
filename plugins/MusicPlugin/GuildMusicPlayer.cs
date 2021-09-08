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
	public sealed class GuildMusicPlayer : IDisposable
	{
		public bool Paused => !_pause.IsSet;
		public MusicApiResponse NowPlaying { get; private set; }
		
		public DiscordChannel CommandChannel { get; private init; }

		private Stream _current;
		private TimeSpan _duration;
		private int _elapsedSeconds;
		
		private readonly SilkApiClient _api;
		private readonly VoiceNextConnection _conn;
		
		
		private readonly AsyncManualResetEvent _pause = new(false);
		
		private CancellationTokenSource _cts = new();
		private CancellationToken _token => _cts.Token;
		
		private Process _ffmpeg;
		private readonly ProcessStartInfo _ffmpegInfo = new()
		{
			Arguments = "-hide_banner -loglevel quiet -i - -map 0:a -ac 2 -f s16le -ar 48k pipe:1",
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			FileName = "./ffmpeg",
			CreateNoWindow = true,
			UseShellExecute = false,
		};
		
		public GuildMusicPlayer(VoiceNextConnection conn, DiscordChannel commandChannel, SilkApiClient api)
		{
			_conn = conn;
			CommandChannel = commandChannel;
			_api = api;
		}

		public async ValueTask PlayAsync()
		{
			if (!Paused) return;
			
			_ffmpeg ??= Process.Start(_ffmpegInfo);
			var sink = _conn.GetTransmitSink();

			await _pause.SetAsync();

			var resuming = true;
			
			if (_current is null)
			{
				resuming = false;
				
				if (!await GetQueuedSongAsync())
				{
					Stop();
					return;
				}
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
			NowPlaying = null;
			_elapsedSeconds = 0;
			_ffmpeg?.Kill();
			_ffmpeg?.Dispose();
			_ffmpeg = null;
		}


		private async Task<bool> GetQueuedSongAsync()
		{
			var current = await _api.PeekNextTrackAsync(CommandChannel.Guild.Id) ?? await _api.GetCurrentTrackAsync(CommandChannel.Guild.Id);
				
			if (current is null) return false; // Empty queue //

			NowPlaying = current;
			_current = new HttpStream(_api, current.Url, await _api.GetContentLength(current.Url), null);
			_duration = current.Duration;

			await _api.GetNextTrackAsync(CommandChannel.Guild.Id);

			return true;
		}
		
		public void Dispose()
		{
			_pause.Reset();
			_cts.Cancel();
			_ffmpeg?.Kill();
			
			
			_current?.Dispose();
			_api?.Dispose();
			_conn?.Dispose();
			_cts?.Dispose();
			_ffmpeg?.Dispose();
		}
	}
}