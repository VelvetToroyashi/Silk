using System;
using System.Buffers;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Emzi0767.Utilities;
using MusicPlugin.Models;
using MusicPlugin.Plugin;

namespace MusicPlugin.Services
{
	public delegate Task MusicTrackEvent(ulong guildId);
	
	public sealed class MusicPlayer : IDisposable
	{
		public MusicApiResponse NowPlaying { get; private set; }

		public event MusicTrackEvent TrackEnded;
		public event MusicTrackEvent TrackEnding;
		
		private readonly DiscordClient _client;
		private readonly DiscordChannel _voice;
		private readonly DiscordChannel _commands;

		private CancellationTokenSource _cts = new();
		private CancellationToken _token => _cts.Token;

		private CancellationTokenSource _playCTS = new();
		private CancellationToken _playToken => _playCTS.Token;

		private VoiceNextConnection _connection;

		private readonly AsyncManualResetEvent _pause = new(true);

		private LazyLoadHttpStream _preloaded;
		
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
		private readonly TimeSpan _preloadBuffer = TimeSpan.FromSeconds(15);

		private readonly SilkApiClient _silkClient;

		public MusicPlayer(MusicConfig config, DiscordClient client, DiscordChannel voice, DiscordChannel commands)
		{
			_client = client;
			_voice = voice;
			_commands = commands;
			_silkClient = new(config);
		}

		public async Task StartAsync()
		{
			_connection = await _voice.ConnectAsync();

			if (_voice.Type is ChannelType.Stage)
			{
				try
				{
					await _voice.UpdateCurrentUserVoiceStateAsync(false);
				}
				catch { await _voice.UpdateCurrentUserVoiceStateAsync(null, DateTimeOffset.Now); }
			}

			await PlayAsync(await GetNextTrackAsync());
		}

		public async Task<MusicApiResponse> GetNextTrackAsync()
		{
			if (NowPlaying is null)
			{
				NowPlaying = await _silkClient.GetCurrentTrackAsync(_voice.Guild.Id);
			}
			else
			{
				NowPlaying = await _silkClient.GetNextTrackAsync(_voice.Guild.Id);
			}

			_preloaded = new(_silkClient, NowPlaying.Url, await _silkClient.GetContentLength(NowPlaying.Url), null);
			await _preloaded.PreloadAsync(CancellationToken.None);
			
			return NowPlaying;
		}

		public async Task PlayAsync(MusicApiResponse music)
		{
			if (music is null)
				return;

			if (!_pause.IsSet)
				return;
			
			_ffmpeg?.Kill();
			_ffmpeg?.Dispose();
			var ffmpeg = _ffmpeg = Process.Start(_ffmpegInfo);

			LazyLoadHttpStream stream = _preloaded ?? new(_silkClient, music.Url, await _silkClient.GetContentLength(music.Url), null);
			
			var sink = _connection.GetTransmitSink();
			var remaining = music.Duration;
			var signaled = false;
			
			var buffer = ArrayPool<byte>.Shared.Rent(8192);
			
			try
			{
				while (remaining >= TimeSpan.Zero && !_playToken.IsCancellationRequested)
				{
					var now = Stopwatch.GetTimestamp();
					
					Console.WriteLine("Beginning of loop");
					if (remaining <= _preloadBuffer && !signaled)
					{
						TrackEnded?.Invoke(_voice.Guild.Id);
						signaled = true;
					}
					
					try
					{
						var t1 = stream.CopyToAsync(ffmpeg.StandardInput.BaseStream, 2048, _token);
						var t2 = ffmpeg?.StandardOutput.BaseStream.CopyToAsync(sink, 2048, _token);

						Task.WhenAll(t1, t2).Wait(_token);
					}
					catch { }
					
					try { await Task.Delay(1000, _token); }
					catch { }
					
					remaining = remaining - TimeSpan.FromSeconds(1);
					await _pause.WaitAsync();
				}

				if (_playToken.IsCancellationRequested)
					return;

				TrackEnded?.Invoke(_voice.Guild.Id);
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(buffer, true);
			}
		}

		public void Pause()
		{
			if (!_pause.IsSet)
				return;
			
			_cts.Cancel();
			_cts.Dispose();
			_pause.Reset();
			_cts = new();
		}		
		
		public async void Resume() => await _pause.SetAsync();
		
		public void Dispose()
		{
			_pause.SetAsync().Wait(CancellationToken.None);
			
			_cts?.Cancel();
			_playCTS?.Cancel();

			_cts?.Dispose();
			_playCTS?.Dispose();
			_connection?.Dispose();
			_preloaded?.Dispose();
			_ffmpeg?.Dispose();
			_silkClient?.Dispose();
		}
	}
}