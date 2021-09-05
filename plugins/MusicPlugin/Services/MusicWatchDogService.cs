using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;

namespace MusicPlugin.Services
{
	public sealed class MusicWatchDogService
	{
		private sealed record ChannelChangeConfirmation(DiscordMessage Message, CancellationTokenSource Token);
		
		private readonly DiscordClient _client;
		private readonly MusicApiService _apiHelper;
		private readonly MusicQueueService _queueHelper;
		private readonly ILogger<MusicWatchDogService> _logger;

		private readonly HashSet<ulong> _disconnectingGuilds = new();

		private readonly Dictionary<ulong, ChannelChangeConfirmation> _guildsPendingDisconnectConfirmation = new();

		public MusicWatchDogService(DiscordClient client, MusicApiService apiHelper, MusicQueueService queueHelper, ILogger<MusicWatchDogService> logger)
		{
			_client = client;
			_logger = logger;
			_queueHelper = queueHelper;
			_apiHelper = apiHelper;

			_client.VoiceStateUpdated += VoiceStateUpdated;
		}
		
		private Task VoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs e)
		{
			if (e.User != _client.CurrentUser)
				return Task.CompletedTask; // Not relevant right now; we'll handle empty channels later. //

			HandleDisconnectAsync(e);
			return Task.CompletedTask;
		}

		public bool IsGuildWaitingForConfirmation(ulong guild) => _guildsPendingDisconnectConfirmation.TryGetValue(guild, out _);

		private async Task HandleDisconnectAsync(VoiceStateUpdateEventArgs args)
		{
			if (_disconnectingGuilds.Contains(args.Guild.Id))
				return;

			if (args.Before is null)
				return;

			_logger.LogDebug("Detected unintentional disconnect in guild, channel: ({Guild}, {Channel})");

			if (args.After is null)
				await HandleChannelDisconnectedAsync(args);
			else await HandleChannelChangedAsync(args);
		}

		private async Task HandleChannelChangedAsync(VoiceStateUpdateEventArgs args)
		{
			if (_guildsPendingDisconnectConfirmation.Remove(args.Guild.Id, out var confirmation))
			{
				try { await confirmation.Message.DeleteAsync(); }
				catch { }
				finally
				{
					confirmation.Token.Cancel();
					confirmation.Token.Dispose(); 
				}
				
				return;
			}
			
			var interactivity = _client.GetInteractivity();
			var builder = new DiscordMessageBuilder()
					.WithContent("**I was playing music in a different channel.**\n" +
								 "Should I clear the queue, or do you want to continue with what was playing?")
					.AddComponents(
						new DiscordButtonComponent(ButtonStyle.Danger, "clear", "Clear the queue"),
						new DiscordButtonComponent(ButtonStyle.Success, "keep", "Keep the queue"));

			var message = await builder.SendAsync(_queueHelper.GetBoundChannelForGuild(args.Guild.Id));
			var cts = new CancellationTokenSource();

			var res = await interactivity.WaitForButtonAsync(message, c => ((DiscordMember)c.User).VoiceState?.Channel == args.Channel, cts.Token);

			_guildsPendingDisconnectConfirmation[args.Guild.Id] = new(message, cts);
			
			if (res.TimedOut) // Canceled //
				return;

			await res.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
			await res.Result.Interaction.DeleteOriginalResponseAsync();
			
			if (res.Result.Id == "clear")
				await _apiHelper.ClearGuildQueueAsync(args.Guild.Id);
			

		}
		
		
		private async Task HandleChannelDisconnectedAsync(VoiceStateUpdateEventArgs args) { }

	}
}