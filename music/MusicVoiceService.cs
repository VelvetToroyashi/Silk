using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.Logging;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Services.Bot.Music
{
	public sealed class MusicVoiceService
	{
		/// <summary>
		/// The states of any given guild.
		/// </summary>
		private readonly ConcurrentDictionary<ulong, MusicState> _states = new();
		private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _watchdogs = new();

		private readonly SemaphoreSlim _lock = new(1);
		
		private readonly DiscordShardedClient _client;
		private readonly ILogger<MusicVoiceService> _logger;
		
		public MusicVoiceService(DiscordShardedClient client, ILogger<MusicVoiceService> logger)
		{
			_client = client;
			_logger = logger;

			_client.VoiceStateUpdated += VoiceStateUpdated;
		}
		~MusicVoiceService() => _client.VoiceStateUpdated -= VoiceStateUpdated;

		public TimeSpan GetRemainingTime(ulong guildId) => _states.TryGetValue(guildId, out var state) ? state.RemainingDuration : TimeSpan.MinValue;
		
		public MusicTrack? GetNowPlaying(ulong guildId)
		{
			if (!_states.TryGetValue(guildId, out var state))
				return null;

			if (state.NowPlaying is null)
				return null;

			else return state.NowPlaying;
		}
		
		/// <summary>
		/// Returns the currently queued tracks for the specified guild.
		/// </summary>
		/// <param name="guildId">The id of the guild to get the tracks for.</param>
		/// <returns>The queued tracks.</returns>
		public async IAsyncEnumerable<MusicTrack> GetQueuedTracksAsync(ulong guildId)
		{
			if (!_states.TryGetValue(guildId, out var state))
				yield break;

			int count = 0;
			
			foreach (var lazy in state.Queue.Queue)
				if (count++ < 9)
					yield return await lazy.Value;
				else yield break;
		}

		#region Connection API
		
		/// <summary>
		/// Joins a new channel.
		/// </summary>
		/// <param name="voiceChannel">The channel to join.</param>
		/// <param name="commandChannel">The channel to send update messages to.</param>
		/// <returns>A <see cref="VoiceResult"/> with the result of trying to join.</returns>
		public async Task<VoiceResult> JoinAsync(DiscordChannel voiceChannel, DiscordChannel commandChannel)
		{
			if (voiceChannel.Type is not (ChannelType.Voice or ChannelType.Stage))
				return VoiceResult.NonVoiceBasedChannel;

			if (!voiceChannel.PermissionsFor(voiceChannel.Guild.CurrentMember).HasPermission(Permissions.Speak | Permissions.UseVoice))
				return VoiceResult.CouldNotJoinChannel;
			
			if (_states.TryGetValue(voiceChannel.Guild.Id, out var state) && state.ConnectedChannel == voiceChannel)
				return VoiceResult.SameChannel;
			
			var vnext = _client.GetShard(voiceChannel.Guild).GetVoiceNext();

			await _lock.WaitAsync();
			
			if (vnext.GetConnection(voiceChannel.Guild) is { } vnextConnection)
				vnextConnection.Disconnect();

			if (!voiceChannel.Guild.CurrentMember.IsDeafened)
			{
				try { await voiceChannel.Guild.CurrentMember.SetDeafAsync(true); }
				catch { }
			}

			var connection = await vnext.ConnectAsync(voiceChannel);
			
			state?.Dispose();
			state = _states[voiceChannel.Guild.Id] = new()
			{
				Connection =  connection,
				CommandChannel = commandChannel
			};
			
			state.TrackEnded += async (s, _) =>
			{
				var state = (MusicState) s!;
				var result = await PlayAsync(state.ConnectedChannel.Guild.Id);

				var builder = new DiscordMessageBuilder();

				if (result is MusicPlayResult.QueueEmpty)
					return;

				var nowPlaying = state.NowPlaying!;
				
				builder.WithContent($"🎶 Now playing: {nowPlaying.Title} - Requested by {nowPlaying.Requester.Mention} " +
				                    $"\nDuration {nowPlaying.Duration:c}, ends at {Formatter.Timestamp(state.RemainingDuration, TimestampFormat.LongTime)}")
					.AddComponents(new DiscordLinkButtonComponent(nowPlaying.Url, "Open song in your browser"))
					.WithoutMentions();
				
				 await state.CommandChannel.SendMessageAsync(builder);
			};
			
			if (voiceChannel.Type is ChannelType.Stage)
			{
				try
				{
					await voiceChannel.UpdateCurrentUserVoiceStateAsync(false);
				}
				catch
				{
					await voiceChannel.UpdateCurrentUserVoiceStateAsync(true, DateTimeOffset.Now);
					_lock.Release();
					return VoiceResult.CannotUnsupress;
				}
			}

			_lock.Release();
			return VoiceResult.Succeeded;
		}

		public async Task LeaveAsync(ulong guildId)
		{
			if (!_states.TryGetValue(guildId, out var state))
			{
				_logger.LogTrace("Not connected to a channel, but LeaveAsync was called.");
				return;
			}
			
			_logger.LogDebug("Disposing of resources and preparring to leave channel.");

			try
			{
				state.Connection.Dispose();
				_logger.LogDebug("Disposed of VNext connection.");
			}
			catch (Exception e)
			{
				_logger.LogTrace(e, "An exception was thrown while disposing of the VNext connection.");
			}
			
			state.Queue.Dispose();
			state.Dispose();
			_states.TryRemove(guildId, out _);
			
			_logger.LogDebug("Disposed of voice-related resources.");
		}

		private async Task VoiceStateUpdated(DiscordClient c, VoiceStateUpdateEventArgs e)
		{
			if (!_states.TryGetValue(e.Guild.Id, out var state))
				return;
			
			if (e.User != _client.CurrentUser)
			{
				if (e.Guild.CurrentMember.VoiceState?.Channel?.Users.Count() == 1)
				{
					_logger.LogDebug("Starting watchdog for {ChannelId}", e.Channel.Id);
					var token = (_watchdogs[e.Guild.Id] = new(TimeSpan.FromMinutes(2))).Token;

					Task.Run(async () =>
					{
						try
						{
							await Task.Delay(TimeSpan.FromMinutes(2), token);
							c.GetVoiceNext().GetConnection(e.Guild)?.Dispose();
							
							state.Queue.Dispose();
							state.Dispose();
							
							_logger.LogDebug("Left VC.");
						}
						catch { }
					});
				}
				else
				{
					if (e.After is null)
						return; // Nothing to do here. //
					if (_watchdogs.TryGetValue(e.Guild.Id, out var watchdog))
					{
						_logger.LogDebug("Cancelling watchdog.");
						watchdog.Cancel();
						watchdog.Dispose();
						_watchdogs.TryRemove(e.Guild.Id, out _);
					}
				}
			}
			else
			{
				// _lock (a SempahoreSlim) will return 0 if the lock is currently being held,
				// which only happens when we're playing a new track *OR* if we're in the process
				// of joining a new channel, in which case we don't want to prematurely dispose of
				// the resources, as that could cause issues up above.
				if (e.After?.Channel is null && _lock.CurrentCount is not 0)
					await LeaveAsync(e.Guild.Id);
				
				else if ((e.Before?.IsSuppressed ?? false) && (!e.After?.IsSuppressed ?? false))
					await PlayAsync(e.Guild.Id);
			}
		}
		
		#endregion
		
		public async Task<MusicPlayResult> PlayAsync(ulong guildId)
		{
			await _lock.WaitAsync();

			try
			{
				if (!_states.TryGetValue(guildId, out var state))
					return MusicPlayResult.InvalidChannel;

				if (state.IsPlaying && state.NowPlaying is not null)
					return MusicPlayResult.AlreadyPlaying;
				
				if (state.NowPlaying is null || state.RemainingDuration <= TimeSpan.Zero)
					if (!await state.Queue.GetNextAsync())
						return MusicPlayResult.QueueEmpty;

				var vnextSink = state.Connection.GetTransmitSink();
			
				await state.ResumeAsync();
				Task yt = state.NowPlaying!.Stream.CopyToAsync(state.InStream, state.Token);
				Task vn = state.OutStream.CopyToAsync(vnextSink, cancellationToken: state.Token);

				_ = Task.Run(async () => { try { await Task.WhenAll(yt, vn); } catch { } });
			
				return MusicPlayResult.NowPlaying;
			}
			finally
			{
				_lock.Release();
			}
		}

		public async Task<MusicPlayResult> SkipAsync(ulong guildId)
		{
			if (!_states.TryGetValue(guildId, out var state))
				return MusicPlayResult.InvalidChannel;
			
			Pause(guildId);

			await state.Queue.GetNextAsync();
			state.RestartFFMpeg();
			
			return await PlayAsync(guildId);
		}

		public void Pause(ulong guildId)
		{
			if (!_states.TryGetValue(guildId, out var state))
				return;
			
			if (!state.IsPlaying)
				return;
			
			state.Pause();
		}

		public async ValueTask ResumeAsync(ulong guildId)
		{
			if (!_states.TryGetValue(guildId, out var state))
				return;

			if (!state.IsPlaying) // Don't needlessly yeet the CT. //
				return;
			
			var vnextSink = state.Connection.GetTransmitSink();
			
			await state.ResumeAsync();
			Task yt = state.NowPlaying!.Stream.CopyToAsync(state.InStream, state.Token);
			Task vn = state.OutStream.CopyToAsync(vnextSink, cancellationToken: state.Token);
			
			_ = Task.Run(async () => await Task.WhenAll(yt, vn));
		}
		
		/// <summary>
		/// Stops the and clears the queue for the specified guild. 
		/// </summary>
		/// <param name="guildId">The guild to stop playback on.</param>
		/// <returns>
		/// <para>A <see cref="MusicPlayResult"/> representing the result of stopping.</para>
		/// <para><see cref="MusicPlayResult.InvalidChannel"/> in the event that music is not playing in that channel.</para>
		/// <para><see cref="MusicPlayResult.QueueEmpty"/> if the queue was properly cleared.</para>
		/// </returns>
		public async Task<MusicPlayResult> StopAsync(ulong guildId)
		{
			if (!_states.TryGetValue(guildId, out var state))
				return MusicPlayResult.InvalidChannel;
			
			if (state.IsPlaying)
				state.Pause();
			
			state.Queue.Dispose();
			state.Dispose();
			return MusicPlayResult.QueueEmpty;
		}
		
		public void Enqueue(ulong guildId, Func<Task<MusicTrack?>> fun)
		{
			if (!_states.TryGetValue(guildId, out var state))
				return;
			
			state.Queue.Enqueue(fun);
		}
	}
	
	public enum VoiceResult
	{
		Succeeded,
		SameChannel,
		CannotUnsupress,
		CouldNotJoinChannel,
		NonVoiceBasedChannel,
	}
}