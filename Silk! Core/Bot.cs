namespace SilkBot
{
    #region Usings
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.Interactivity;
    using DSharpPlus.Interactivity.Enums;
    using DSharpPlus.Interactivity.Extensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NLog;
    using NLog.Conditions;
    using NLog.Config;
    using NLog.Extensions.Logging;
    using NLog.Targets;
    using Silk__Extensions;
    using SilkBot.Commands.Bot;
    using SilkBot.Commands.General;
    using SilkBot.Services;
    using SilkBot.Tools;
    using SilkBot.Utilities;
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    #endregion
    public class Bot
    {
        #region Props
        public DiscordClient Client { get; set; }
        public static Bot Instance { get; private set; } 
        public static DateTime StartupTime { get; } = DateTime.Now;
        public static string SilkDefaultCommandPrefix { get; } = "!";
        public static Stopwatch CommandTimer { get; } = new Stopwatch();
        public SilkDbContext SilkDBContext { get; } = new SilkDbContext();
        public TimerBatcher Timer { get; } = new TimerBatcher(new ActionDispatcher());
        
        private ServiceProvider Services;
        

        public CommandsNextConfiguration Commands { get; private set; }

        #endregion
        private ILogger<Bot> _logger;
        private readonly Stopwatch _sw = new Stopwatch();

        public Bot() 
        {
            _sw.Start();
            Instance = this;
        }
        #region Methods
        public async Task RunBotAsync(ServiceCollection services)
        {
            SetupNLog();

            try
            {
                await SilkDBContext.Database.MigrateAsync();
            }
            catch (Npgsql.PostgresException)
            {
                Colorful.Console.WriteLine($"Database: Invalid password. Is the password correct, and did you setup the database?", Color.Red);
                Environment.Exit(1);
            }

            await InitializeClientAsync(services);

            RegisterCommands();

            await Task.Delay(-1);
        }

        private void SetupNLog()
        {
            var config = new LoggingConfiguration();
            var consoleTarget = new ColoredConsoleTarget
            {
                Name = "console",
                EnableAnsiOutput = true,
                Layout = "$[${level}] \u001b[0m${message}",
                UseDefaultRowHighlightingRules = false,

            };
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Trace"), ConsoleOutputColor.Green, ConsoleOutputColor.Black));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Debug"), ConsoleOutputColor.Green, ConsoleOutputColor.Black));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Warn"), ConsoleOutputColor.Blue, ConsoleOutputColor.Black));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Error"), ConsoleOutputColor.Red, ConsoleOutputColor.Black));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Fatal"), ConsoleOutputColor.DarkRed, ConsoleOutputColor.Black));

            config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Error, consoleTarget, "*");
            LogManager.Configuration = config;


        }

        private void AddServices(ServiceCollection services)
        {
            Services = services
                .AddSingleton(Client)
                .BuildServiceProvider();
            _logger = Services.Get<ILogger<Bot>>();
            _logger.LogInformation("All services initalized.");
            Client.MessageCreated += Services.Get<MessageCreationHandler>().OnMessageCreate;
            new BotEventHelper(Client, Services.Get<IDbContextFactory<SilkDbContext>>(), Services.Get<ILogger<BotEventHelper>>());//.CreateHandlers(Client);
        }





        private void RegisterCommands()
        {
            var sw = Stopwatch.StartNew();
            Client.GetCommandsNext().RegisterCommands(Assembly.GetExecutingAssembly());
            sw.Stop();
            _logger.LogDebug($"Registered all commands in {sw.ElapsedMilliseconds} ms.");
        }

        private async Task InitializeClientAsync(ServiceCollection services)
        {
            var token = File.ReadAllText("./Token.txt");
            var config = new DiscordConfiguration
            {
                AutoReconnect = true,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.None,
                Token = token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.Guilds,
                LogTimestampFormat = "H:mm:sstt"
            };

            Client = new DiscordClient(config);
            
            AddServices(services);
            Commands = new CommandsNextConfiguration { PrefixResolver = Services.Get<PrefixCacheService>().PrefixDelegate,  Services = Services };

            Client.UseInteractivity(new InteractivityConfiguration { PaginationBehaviour = PaginationBehaviour.WrapAround, Timeout = TimeSpan.FromMinutes(1), PaginationDeletion = PaginationDeletion.DeleteMessage });
            Client.UseCommandsNext(Commands);
            //Client.GetCommandsNext().SetHelpFormatter<HelpFormatter>();
            await Task.Delay(100);
            _logger.LogInformation("Client Initialized.");
            


            await Client.ConnectAsync();

            _sw.Stop();
            _logger.LogInformation($"Startup time: {_sw.Elapsed.Seconds} seconds.");
            Client.Ready += (c, e) => { _logger.LogInformation("Client ready to proccess commands."); return Task.CompletedTask; };
        }


        #endregion
    }
}
