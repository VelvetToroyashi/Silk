using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Silk.Core.EventHandlers.Messages;
using Silk.Core.SlashCommands;
using Silk.Core.Utilities;
using Silk.Core.Utilities.Bot;
using Silk.Core.Utilities.HelpFormatter;

namespace Silk.Core
{
    public class Main : IHostedService
    {
        //public static DiscordSlashClient SlashClient { get; } // Soon™ //
        public DiscordShardedClient ShardClient { get; }
        public static string DefaultCommandPrefix => "s!";

        private static ILogger<Main> _logger;
        private readonly BotExceptionHandler _handler;
        private readonly CommandHandler _commandHandler;

        public Main(DiscordShardedClient shardClient, ILogger<Main> logger, EventHelper e, BotExceptionHandler handler, CommandHandler commandHandler) // About the EventHelper: Consuming it in the ctor causes it to be constructed,
        {
            // And that's all it needs, since it subs to events in it's ctor.
            _logger = logger; // Not ideal, but I'll figure out a better way. Eventually. //
            _handler = handler;
            _commandHandler = commandHandler;
            ShardClient = shardClient;
            _ = e;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting service");
            await InitializeClientExtensions();
            _logger.LogInformation("Initialized client");
            await InitializeCommandsNextAsync();
            _logger.LogInformation("Initialized CommandsNext");
            await InitializeSlashCommandsAsync();
            _logger.LogInformation("Initialized Slash-Commands");
            await _handler.SubscribeToEventsAsync();
            _logger.LogDebug("Connecting to Discord gateway");
            await ShardClient.StartAsync();
            _logger.LogInformation("Connected to Discord gateway as {Username}#{Discriminator}", ShardClient.CurrentUser.Username, ShardClient.CurrentUser.Discriminator);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping service");
            _logger.LogDebug("Disconnecting from Discord gateway");
            await ShardClient.StopAsync();
            _logger.LogInformation("Disconnected from Discord gateway");
        }

        private async Task InitializeClientExtensions()
        {
            _logger.LogDebug("Initializing client");

            await ShardClient.UseCommandsNextAsync(DiscordConfigurations.CommandsNext);
            await ShardClient.UseInteractivityAsync(DiscordConfigurations.Interactivity);
        }

        private async Task InitializeSlashCommandsAsync()
        {
            var sc = ShardClient.ShardClients[0].UseSlashCommands(DiscordConfigurations.SlashCommands);

            sc.RegisterCommands<RemindCommands>(722169538178973766);

            sc.SlashCommandErrored += async (c, e) => _logger.LogCritical("Slash command errored! Exception: {Ex}", e.Exception);
        }

        private async Task InitializeCommandsNextAsync()
        {
            _logger.LogDebug("Registering commands");

            var t = Stopwatch.StartNew();
            var asm = Assembly.GetEntryAssembly();
            IReadOnlyDictionary<int, CommandsNextExtension>? cnext = await ShardClient.GetCommandsNextAsync();

            foreach (var cnextExt in cnext.Values)
            {
                cnextExt.RegisterCommands(asm);
                cnextExt.SetHelpFormatter<HelpFormatter>();
                cnextExt.RegisterConverter(new MemberConverter());
                cnextExt.CommandExecuted += _commandHandler.AddCommandInvocation;
            }

            t.Stop();
            int registeredCommands = cnext.Values.Sum(r => r.RegisteredCommands.Count);

            _logger.LogDebug("Registered {Commands} commands for {Shards} shards in {Time} ms", registeredCommands, ShardClient.ShardClients.Count, t.ElapsedMilliseconds);
        }
    }
}