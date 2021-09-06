using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using MusicPlugin.Plugin;

namespace MusicPlugin.Services
{
	public sealed class MusicPlayer
	{
		private readonly DiscordClient _client;
		private readonly DiscordChannel _voice;
		private readonly DiscordChannel _commands;

		private VoiceNextConnection _connection;

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
		private readonly TimeSpan _preloadBuffer = TimeSpan.FromSeconds(15);

		private readonly SilkApiClient _silkClient;

		private TaskCompletionSource _tcs = new();
		
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

			_ = DurationLoopAsync();
		}

		private async Task DurationLoopAsync()
		{
			var current = await _silkClient.GetCurrentTrackAsync(_voice.Guild.Id);
			var sink = _connection?.GetTransmitSink() ?? throw new InvalidOperationException("Transmit loop was invoked outside of a voice channel.");
			
			while (!_token.IsCancellationRequested)
			{
				while (current is null)
				{
					current = await _silkClient.GetNextTrackAsync(_voice.Guild.Id);
					await Task.Delay(700, _token);
					
					if (_token.IsCancellationRequested) 
						return;
				}
				
				await using var stream = new LazyLoadHttpStream(_silkClient, current.Url, await _silkClient.GetContentLength(current.Url), null);
				_ffmpeg = Process.Start(_ffmpegInfo);

				var vnextTask = _ffmpeg.StandardOutput.BaseStream.CopyToAsync(sink, 8192, _token);
				var apiTask = stream.CopyToAsync(_ffmpeg.StandardInput.BaseStream, 8192, _token);
				
				while (stream.Position != stream.Length)
				{
					await Task.Delay(TimeSpan.FromSeconds(1), _token);
					
					if (_token.IsCancellationRequested) 
						return;
				}
				
				await Task.WhenAll(vnextTask, apiTask);
				await sink.FlushAsync(_token);

				if (!_ffmpeg?.HasExited ?? false)
				{
					_ffmpeg.Kill();
					_ffmpeg.Dispose();
				}
			}
		}
	}
}