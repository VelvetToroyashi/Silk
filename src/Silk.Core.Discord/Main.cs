using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Silk.Core.Discord.Types;

namespace Silk.Core.Discord
{
    public class Main : IHostedService
    {
        //public static DiscordSlashClient SlashClient { get; } // Soon™ //
        public static DiscordShardedClient ShardClient { get; }
        public static BotState State { get; private set; }

        private readonly ILogger<Bot> _logger;
        private readonly IMediator _mediator;
        private readonly IServiceProvider _services;


        public async Task StartAsync(CancellationToken cancellationToken) { }
        public async Task StopAsync(CancellationToken cancellationToken) { }
    }
}