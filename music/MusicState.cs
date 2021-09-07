using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Emzi0767.Utilities;

namespace Silk.Core.Services.Bot.Music
{
	internal sealed record MusicState : IDisposable
	{
		//TODO: Volume, Skip(int x), BassBoost?
		
		/// <summary>
		/// The queue'd songs for this particular state.
		/// </summary>
		public MusicQueue Queue { get; } = new();
		
		/// <summary>
		/// The currently playing song, if any.
		/// </summary>
		public MusicTrack? NowPlaying => Queue.NowPlaying;
		
		/// <summary>
		/// The remaining duration of the song.
		/// </summary>
		public TimeSpan RemainingDuration => Queue.RemainingDuration;
	
		/// <summary>
		/// The connection to Discord associated with this state.
		/// </summary>
		public VoiceNextConnection Connection { get; init; }
		
		/// <summary>
		/// The channel this state was invoked from. Update messages will be sent back through this channel.
		/// </summary>
		public DiscordChannel CommandChannel { get; init; }
		
		/// <summary>
		/// The channel this state is playing music to.
		/// </summary>
		public DiscordChannel ConnectedChannel => Connection.TargetChannel;

		/// <summary>
		/// A public cacellation token used to cancelling CopyToAsync.
		/// </summary>
		public CancellationToken Token => _cts.Token;
		private CancellationTokenSource _cts = new();
		
		/// <summary>
		/// Whether this state is in a play state.
		/// </summary>
		public bool IsPlaying => _mre.IsSet;

		private readonly AsyncManualResetEvent _mre = new(false);

		/// <summary>
		/// Source -> FFMpeg
		/// </summary>
		public FileStream InStream => (FileStream)_ffmpeg.StandardInput.BaseStream;
		
		/// <summary>
		/// FFMPeg -> VNext
		/// </summary>
		public FileStream OutStream => (FileStream)_ffmpeg.StandardOutput.BaseStream;
		
		private Process _ffmpeg;
		private readonly ProcessStartInfo _ffmpegInfo = new()
		{
			Arguments = "-hide_banner -loglevel quiet -i - -map 0:a -ac 2 -f s16le -ar 48k pipe:1 ",
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			FileName =
				OperatingSystem.IsLinux() ? "./ffmpeg" : //Linux is just ffmpeg
				OperatingSystem.IsWindows() ? "./ffmpeg-windows" :
				throw new PlatformNotSupportedException(),
			CreateNoWindow = true,
			UseShellExecute = false,
		};
		private readonly TimeSpan _preloadBuffer = TimeSpan.FromSeconds(10);
		private bool _disposing;

		public MusicState()
		{
			RestartFFMpeg();
			DoDurationLoopAsync();
		}
		
		/// <summary>
		/// Resumes the playback timer.
		/// </summary>
		public Task ResumeAsync() => _mre.SetAsync();
		
		/// <summary>
		/// An event that fires when the current playing track ends.
		/// </summary>
		public event EventHandler TrackEnded;
		
		/// <summary>
		/// Pauses the playback timer and cancels the <see cref="Token"/>.
		/// </summary>
		public void Pause()
		{
			_mre.Reset();
			_cts.Cancel();
			_cts.Dispose();
			_cts = new();
		}
		
		/// <summary>
		/// Restarts FFMpeg.4
		/// </summary>
		public void RestartFFMpeg()
		{
			_ffmpeg?.Kill();
			_ffmpeg?.Dispose();
			_ffmpeg = Process.Start(_ffmpegInfo)!;
		}
		
		/// <summary>
		/// Loops roughly every second while <see cref="_mre"/> isn't set,
		/// checking if the current track is about to end, and preloading the next one.
		///
		/// <para>This also fires <see cref="TrackEnded"/> which can be used for auto-play.</para>
		/// </summary>
		private async void DoDurationLoopAsync()
		{
			var trackLoaded = false;
			while (!_disposing)
			{
				await _mre.WaitAsync();
			
				// The reson CT.None is passed is because pausing and resuming sets the MRE,
				// so we don't want to cancel, becasue we're going to wait anyway.
				await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);
				
				if (RemainingDuration < _preloadBuffer && !trackLoaded)
				{
					if (Queue.RemainingTracks is not 0)
					{
						await Queue.PreloadAsync();
						trackLoaded = true;
					}
				}

				if (RemainingDuration <= TimeSpan.Zero)
				{
					Pause();
					RestartFFMpeg(); // FFMpeg(?) cuts off the beginning of the song after the first song otherwise. //
					trackLoaded = false;
					TrackEnded(this, EventArgs.Empty); //TODO: Make custom handler 
				}
				
				Queue.RemainingSeconds--;
			}
		}

		~MusicState() => Dispose();
		
		public void Dispose()
		{
			if (_disposing)
				throw new ObjectDisposedException(this.GetType().Name, "This object is already disposed.");
			_disposing = true;
			GC.SuppressFinalize(this);
			_cts.Dispose();
			_ffmpeg?.Kill();
			_ffmpeg?.Dispose();
			Connection?.Dispose();
		}
	}
}