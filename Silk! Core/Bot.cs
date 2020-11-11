namespace SilkBot
{
    #region Usings
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.Interactivity;
    using DSharpPlus.Interactivity.Enums;
    using DSharpPlus.Interactivity.Extensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using NLog;
    using NLog.Conditions;
    using NLog.Config;
    using NLog.Targets;
    using SilkBot.Extensions;
    using SilkBot.Services;
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

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

            await InitializeCommands();

            await Task.Delay(-1);
        }

        //private void SetupNLog()
        //{
        //    var config = new LoggingConfiguration();
        //    var consoleTarget = new ColoredConsoleTarget
        //    {
        //        Name = "console",
        //        EnableAnsiOutput = true,
        //        Layout = "$[${level}] \u001b[0m${message}",
        //        UseDefaultRowHighlightingRules = false,

        //    };
        //    consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Trace"), ConsoleOutputColor.Green, ConsoleOutputColor.Black));
        //    consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Debug"), ConsoleOutputColor.Green, ConsoleOutputColor.Black));
        //    consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Warn"), ConsoleOutputColor.Blue, ConsoleOutputColor.Black));
        //    consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Error"), ConsoleOutputColor.Red, ConsoleOutputColor.Black));
        //    consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Fatal"), ConsoleOutputColor.DarkRed, ConsoleOutputColor.Black));

        //    config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Error, consoleTarget, "*");
        //    LogManager.Configuration = config;


        //}

        //private void AddServices(ServiceCollection services)
        //{
        //    Services = services
        //        .AddSingleton(Client)
        //        .AddSingleton<MessageCreationHandler>()
        //        .BuildServiceProvider();
        //    _logger = Services.Get<ILogger<Bot>>();
        //    _logger.LogInformation("All services initalized.");
        //    Client.MessageCreated += Services.Get<MessageCreationHandler>().OnMessageCreate;
        //    new BotEventHelper(Client, Services.Get<IDbContextFactory<SilkDbContext>>(), Services.Get<ILogger<BotEventHelper>>());//.CreateHandlers(Client);
        //}





        private Task InitializeCommands()
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

            Commands = new CommandsNextConfiguration { PrefixResolver = _services.Get<PrefixCacheService>().PrefixDelegate, Services = _services };
            await Client.UseInteractivityAsync(new InteractivityConfiguration
            {
                PaginationBehaviour = PaginationBehaviour.WrapAround,
                Timeout = TimeSpan.FromMinutes(1),
                PaginationDeletion = PaginationDeletion.DeleteMessage
            });
            await Client.UseCommandsNextAsync(Commands);
            //Client.GetCommandsNext().SetHelpFormatter<HelpFormatter>();
            await Task.Delay(100);
            _logger.LogInformation("Client Initialized.");

            await Client.StartAsync();

            _sw.Stop();
            _logger.LogInformation($"Startup time: {_sw.Elapsed.Seconds} seconds.");
            Client.Ready += (c, e) => { _logger.LogInformation("Client ready to proccess commands."); return Task.CompletedTask; };
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var config = new LoggingConfiguration();
            var consoleTarget = new ColoredConsoleTarget
            {
                Name = "console",
                EnableAnsiOutput = true,
                Layout = "[${level}] \u001b[0m${message}",
                UseDefaultRowHighlightingRules = false,

            };
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Info"), ConsoleOutputColor.Cyan, ConsoleOutputColor.Black));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Debug"), ConsoleOutputColor.Green, ConsoleOutputColor.Black));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Warn"), ConsoleOutputColor.Blue, ConsoleOutputColor.Black));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Error"), ConsoleOutputColor.Red, ConsoleOutputColor.Black));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Fatal"), ConsoleOutputColor.DarkRed, ConsoleOutputColor.Black));

            config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Error, consoleTarget, "*");
            LogManager.Configuration = config;
            await RunBotAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Client.StopAsync();
        }


        #endregion
    }
}
