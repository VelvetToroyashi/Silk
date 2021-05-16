using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using Emzi0767.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Silk.Core.Services
{
    public class UptimeService : BackgroundService
    {
        public DateTime LastOutage { get; private set; }
        public TimeSpan OutageTime { get; private set; }

        public DateTime UpTime { get; private set; } = Process.GetCurrentProcess().StartTime;

        private bool _isUp = true;

        private readonly AsyncManualResetEvent _reset;
        private readonly ILogger<UptimeService> _logger;
        public UptimeService(DiscordShardedClient client, ILogger<UptimeService> logger)
        {
            DiscordShardedClient client1 = client;
            _logger = logger;
            _reset = new(false);

            client1.SocketOpened += async (_, _) =>
            {
                _isUp = true;
                _ = _reset.SetAsync();
            };
            client1.SocketClosed += async (_, _) =>
            {
                _isUp = false;
                _reset.Reset();
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield(); // We need an await, else we hold everything up. https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-5.0&tabs=visual-studio#backgroundservice-base-class //
            _logger.LogInformation("Started!");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (!_isUp)
                {
                    var down = DateTime.Now;

                    await _reset.WaitAsync();
                    TimeSpan downTime = DateTime.Now - down;

                    if (downTime > TimeSpan.FromSeconds(1))
                    {
                        LastOutage = down;
                        OutageTime = downTime;
                    }

                    UpTime = UpTime - downTime;
                }
            }

            _logger.LogInformation("Stopping service");
        }
    }
}