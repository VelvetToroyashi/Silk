using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Silk.Core.Types;
using Silk.Core.Utilities;
using Silk.Core.Utilities.Bot;
using Silk.Core.Utilities.HelpFormatter;

namespace Silk.Core
{
    public class Main : IHostedService
    {
        public BotState State { get; private set; } = BotState.Starting;
        //public static DiscordSlashClient SlashClient { get; } // Soon™ //
        public DiscordShardedClient ShardClient { get; }
        public static string DefaultCommandPrefix => "s!";

        private static ILogger<Main> _logger;
        private readonly BotExceptionHandler _handler;

        public Main(DiscordShardedClient shardClient, ILogger<Main> logger, EventHelper e, BotExceptionHandler handler) // About the EventHelper: Consuming it in the ctor causes it to be constructed,
        {
            // And that's all it needs, since it subs to events in it's ctor.
            _logger = logger; // Not ideal, but I'll figure out a better way. Eventually. //
            _handler = handler;
            ShardClient = shardClient;
            _ = e;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ChangeState(BotState state)
        {
            if (State == state) return;

            _logger.LogDebug("State changed from {State} to {NewState}!", State, state);
            State = state;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting service");
            await InitializeClientExtensions();
            await InitializeCommandsNextAsync();
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



        private async Task InitializeCommandsNextAsync()
        {
            _logger.LogDebug("Registering commands");

            var t = Stopwatch.StartNew();
            var asm = Assembly.GetEntryAssembly();
            var cnext = await ShardClient.GetCommandsNextAsync();

            foreach (var cnextExt in cnext.Values)
            {
                cnextExt.RegisterCommands(asm);
                cnextExt.SetHelpFormatter<HelpFormatter>();
                cnextExt.RegisterConverter(new MemberConverter());
            }

            t.Stop();
            var registeredCommands = cnext.Values.Sum(r => r.RegisteredCommands.Count);

            _logger.LogDebug("Registered {Commands} commands for {Shards} shards in {Time} ms", registeredCommands, ShardClient.ShardClients.Count, t.ElapsedMilliseconds);
        }
    }
}