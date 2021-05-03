using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Silk.Core.Discord.Types;
using Silk.Core.Discord.Utilities;
using Silk.Core.Discord.Utilities.Bot;
using Silk.Core.Discord.Utilities.HelpFormatter;

namespace Silk.Core.Discord
{
    public class Main : IHostedService
    {
        public static BotState State { get; private set; } = BotState.Starting;
        //public static DiscordSlashClient SlashClient { get; } // Soon™ //
        public static DiscordShardedClient ShardClient { get; private set; }
        public static string DefaultCommandPrefix { get; } = "s!";

        private static ILogger<Main> _logger;

        public Main(DiscordShardedClient shardClient, ILogger<Main> logger)
        {
            ShardClient = shardClient;
            _logger = logger;
        }

        public static void ChangeState(BotState state)
        {
            if (State == state) return;

            _logger.LogDebug("State changed from {State} to {NewState}!", State, state);
            State = state;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting service");
            await InitializeClientAsync();
            await InitializeCommandsNextAsync();
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

        private async Task InitializeClientAsync()
        {
            _logger.LogDebug("Initializing client");

            await ShardClient.UseCommandsNextAsync(DiscordConfigurations.CommandsNext);
            await ShardClient.UseInteractivityAsync(DiscordConfigurations.Interactivity);

        }
        private async Task InitializeCommandsNextAsync()
        {
            _logger.LogDebug("Registering commands");

            var t = Stopwatch.StartNew();
            var asm = Assembly.GetExecutingAssembly();
            var cnext = await ShardClient.GetCommandsNextAsync();

            foreach (var cnextExt in cnext.Values)
            {
                cnextExt.RegisterCommands(asm);
                cnextExt.SetHelpFormatter<HelpFormatter>();
                cnextExt.RegisterConverter(new MemberConverter());
            }

            t.Stop();
            var registeredCommands = cnext[0].RegisteredCommands.Count;

            _logger.LogDebug("Registered {Commands} commands for {Shards} shards in {Time} ms", registeredCommands, ShardClient.ShardClients.Count, t.ElapsedMilliseconds);
        }


    }
}