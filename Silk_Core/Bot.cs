#region Usings
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SilkBot.Commands.Bot;
using SilkBot.Extensions;
using SilkBot.Services;
using SilkBot.Utilities;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SilkBot
{
    #endregion
    public class Bot : IHostedService
    {
        #region Props
        public DiscordShardedClient Client { get; set; }
        public static Bot Instance { get; private set; }
        public static DateTime StartupTime { get; } = DateTime.Now;
        public static string SilkDefaultCommandPrefix { get; } = "!";
        public static Stopwatch CommandTimer { get; } = new Stopwatch();
        public SilkDbContext SilkDBContext { get; private set; }
        public Task ShutDownTask { get => ShutDownTask; set { if (ShutDownTask is not null) return; } }
        private IServiceProvider _services;


        public CommandsNextConfiguration Commands { get; private set; }

        #endregion
        private ILogger<Bot> _logger;
        private readonly Stopwatch _sw = new Stopwatch();

        public Bot(IServiceProvider services, DiscordShardedClient client)
        {
            _sw.Start();
            _services = services;
            _logger = _services.Get<ILogger<Bot>>();
            client.MessageCreated += services.Get<MessageCreationHandler>().OnMessageCreate;
            SilkDBContext = _services.Get<IDbContextFactory<SilkDbContext>>().CreateDbContext(); // Anti-pattern according to some, but it might work. //
            Instance = this;
            Client = client;
            
        }
        #region Methods
        public async Task RunBotAsync()
        {

            try
            {
                await SilkDBContext.Database.MigrateAsync();
            }
            catch (Npgsql.PostgresException)
            {
                Colorful.Console.WriteLine($"Database: Invalid password. Is the password correct, and did you setup the database?", Color.Red);
                Environment.Exit(1);
            }

            await InitializeClientAsync();

            InitializeCommands();
            
            await Task.Delay(-1);
        }

        private void InitializeCommands()
        {
            var sw = Stopwatch.StartNew();
            foreach (var shard in Client.ShardClients.Values)
            {
                var cmdNext = shard.GetCommandsNext();
                cmdNext.RegisterCommands(Assembly.GetExecutingAssembly());
            }
            sw.Stop();
            _logger.LogDebug($"Registered commands for {Client.ShardClients.Count()} shards in {sw.ElapsedMilliseconds} ms.");
        }

        private async Task InitializeClientAsync()
        {
            _services.Get<BotEventHelper>().CreateHandlers();
            Commands = new CommandsNextConfiguration { PrefixResolver = _services.Get<PrefixCacheService>().PrefixDelegate, Services = _services };
            await Client.UseInteractivityAsync(new InteractivityConfiguration
            {
                PaginationBehaviour = PaginationBehaviour.WrapAround,
                Timeout = TimeSpan.FromMinutes(1),
                PaginationDeletion = PaginationDeletion.DeleteMessage
            });
            await Client.UseCommandsNextAsync(Commands);
            var cmdNext = await Client.GetCommandsNextAsync();
            foreach (CommandsNextExtension c in cmdNext.Values) c.SetHelpFormatter<HelpFormatter>();
            foreach (var c in cmdNext.Values) c.RegisterConverter(new MemberConverter());
            _logger.LogInformation("Client Initialized.");

            await Client.StartAsync();

            Client.GuildDownloadCompleted
                += async (_, _) =>
                {
                    _logger.LogDebug("Starting cache run");
                    foreach(Task t in BotEventHelper.CacheStaff)
                    {
                        _ = t.GetAwaiter();
                    }
                    _logger.LogInformation("Cache run complete.");
                };
            _sw.Stop();
            _logger.LogInformation($"Startup time: {_sw.Elapsed.Seconds} seconds.");
            Client.Ready += (c, e) => { _logger.LogInformation("Client ready to proccess commands."); return Task.CompletedTask; };
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await RunBotAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Client.StopAsync();
        }


        #endregion
    }
}
