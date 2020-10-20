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
    using NLog.Fluent;
    using NLog.Targets;
    using Silk__Extensions;
    using SilkBot.Commands.Bot;
    using SilkBot.Commands.General;
    using SilkBot.Services;
    using SilkBot.Tools;
    using SilkBot.Utilities;
    using System;
    using System.Collections.Generic;
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
        public static Bot Instance { get; } = new Bot();
        public static DateTime StartupTime { get; } = DateTime.Now;
        public static string SilkDefaultCommandPrefix { get; } = "!";
        public static Stopwatch CommandTimer { get; } = new Stopwatch();
        public SilkDbContext SilkDBContext { get; } = new SilkDbContext();
        public TimerBatcher Timer { get; } = new TimerBatcher(new ActionDispatcher());

        private ServiceProvider Services;

        public CommandsNextConfiguration Commands { get; private set; }
        
        #endregion

        private readonly Stopwatch sw = new Stopwatch();

        private Bot() => sw.Start();
        #region Methods
        public async Task RunBotAsync()
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
            
            await InitializeClientAsync();
            
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
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Info"), ConsoleOutputColor.Cyan, ConsoleOutputColor.Black));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Debug"), ConsoleOutputColor.Green, ConsoleOutputColor.Black));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Warn"), ConsoleOutputColor.Blue, ConsoleOutputColor.Black));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Error"), ConsoleOutputColor.Red, ConsoleOutputColor.Black));
            consoleTarget.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Fatal"), ConsoleOutputColor.DarkRed, ConsoleOutputColor.Black));

            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Error, consoleTarget, "*");
            LogManager.Configuration = config;

        
        }

        private void AddServices()
        {
            
            Services = new ServiceCollection()

                //.AddSingleton<DiscordEmojiCreationService>()
                .AddMemoryCache(option => option.ExpirationScanFrequency = TimeSpan.FromHours(1))
                .AddSingleton<NLogLoggerFactory>()
                .AddSingleton<PrefixCacheService>()
                .AddSingleton<MessageCreationHandler>()
                .AddScoped<BotEventHelper>()
                .AddSingleton<TicketService>()
                .AddDbContextFactory<SilkDbContext>(lifetime: ServiceLifetime.Transient)
                .AddLogging(lg => 
                {
                    lg.ClearProviders();
                    lg.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    lg.AddNLog("./NLog.config");
                })

                //.AddSingleton<GuildConfigCacheService>()
                .AddSingleton(Client)
                .BuildServiceProvider();

            Client.MessageCreated += Services.Get<MessageCreationHandler>().OnMessageCreate;
        }





        private void RegisterCommands() => Client.GetCommandsNext().RegisterCommands(Assembly.GetExecutingAssembly());

        private async Task InitializeClientAsync()
        {
            var token = File.ReadAllText("./Token.txt");
            var config = new DiscordConfiguration
            {
                AutoReconnect = true,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Information,
                Token = token,
                TokenType = TokenType.Bot,
                
            };

            Client = new DiscordClient(config);
            AddServices();
            Commands = new CommandsNextConfiguration { EnableDefaultHelp = false, UseDefaultCommandHandler = false, Services = Services, EnableMentionPrefix = true };
            Client.UseInteractivity(new InteractivityConfiguration { PaginationBehaviour = PaginationBehaviour.WrapAround, Timeout = TimeSpan.FromMinutes(1)});

            Client.UseCommandsNext(Commands);

            // Register database context and apply newest migration if not already done.

            
            HelpCache.Initialize();
            
            await Client.ConnectAsync();

            sw.Stop();
            Colorful.Console.WriteLine($"Startup time: {(sw.ElapsedMilliseconds / 1000d):F2} seconds", Color.CornflowerBlue);
        }


        #endregion
    }
}
