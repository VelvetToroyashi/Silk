using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Silk.Shared.Types.Collections;

namespace Silk.Core.Discord.Services
{
    public class StatusService : BackgroundService
    {
        private bool _ready = false;

        private readonly DiscordShardedClient _client;
        private readonly ILogger<StatusService> _logger;
        private readonly LoopedList<string> _statuses = new()
        {
            "for s!help",
            "for @Silk help",
            "you!",
            "cute red pandas",
            "for commands!"
        };


        public StatusService(DiscordShardedClient client, ILogger<StatusService> logger)
        {
            _client = client;
            _logger = logger;

            client.Ready += (_, _) =>
            {
                _ready = true;
                return Task.CompletedTask;
            };
        }

        // This service starts before the client actually connects to Discord, so we need to wait. //
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogWarning("Waiting for client to connect to gateway");
            while (!_ready) { await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken); }
            _logger.LogInformation("Started!");

            try
            {
                while (true)
                {
                    var activity = new DiscordActivity(_statuses.Next(), ActivityType.Watching);
                    try { await _client.UpdateStatusAsync(activity); }
                    catch (SocketException) { }
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }
            catch (TaskCanceledException) { }
            finally
            {
                _logger.LogDebug("Cancelation requested. Stopping service");
            }
        }
    }
}