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
	public sealed class MusicService
	{
		private Stream _current;
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
		
		public MusicService(VoiceNextConnection conn, DiscordChannel commandChannel, SilkApiClient api)
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
			
			if (_current is null)
			{
				var current = await _api.GetNextTrackAsync(_commandChannel.Guild.Id);

				_current = new HttpStream(_api, current.Url, await _api.GetContentLength(current.Url), null);
			}

			_ = Task.Run(async () =>
			{
				var s = _current.CopyToAsync(_ffmpeg.StandardInput.BaseStream, _token);
				var v = _ffmpeg.StandardOutput.BaseStream.CopyToAsync(sink, cancellationToken: _token);

				try { await Task.WhenAll(s, v); }
				catch { }
			}, CancellationToken.None);
		}

		private async void AutoPlayLoopAsync()
		{
			while (true)
			{
				await Task.Delay(1000);
				await _pause.WaitAsync();

				if (_current.Position == _current.Length)
				{
					_current = null;
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

	}
}