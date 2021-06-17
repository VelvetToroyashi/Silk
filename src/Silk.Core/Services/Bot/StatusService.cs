using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Emzi0767.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Silk.Shared.Types.Collections;

namespace Silk.Core.Services.Bot
{
    public sealed class StatusService : BackgroundService
    {
        private readonly DiscordShardedClient _client;
        private readonly ILogger<StatusService> _logger;
        private readonly AsyncManualResetEvent _tcs = new(false);
        private readonly LoopedList<string> _statuses = new()
        {
            "for s!help",
            "for @Silk help",
            "you!",
            "cute red pandas",
            "for commands!",
            "patreon/VelvetThePanda",
            "ko-fi.com/VelvetThePanda",
            "for donations! (ko-fi/patreon: VelvetThePanda)"
        };

        public StatusService(DiscordShardedClient client, ILogger<StatusService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public void Clear() => _statuses.Clear();
        public void AddOption(string option) => _statuses.Add(option);
        public bool RemoveOption(string option) => _statuses.Remove(option);



        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Started!");

            try
            {
                while (true)
                {
                    var activity = new DiscordActivity(_statuses.Next(), ActivityType.Watching);
                    try { await _client.UpdateStatusAsync(activity); }
                    catch (SocketException) { }
                    catch (NullReferenceException) { } // Websocket isn't initialized. Why this fires before all shards are ready is beyond me ~Velvet //
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }
            catch (TaskCanceledException) { }
            finally
            {
                _logger.LogDebug("Cancelation requested. Stopping service.");
            }
        }
    }
}