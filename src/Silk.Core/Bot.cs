using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Silk.Core.Database;
using Silk.Core.Utilities;

namespace Silk.Core
{

    public class Bot : IHostedService
    {
        //TODO: Fix all these usages, because they should be pulling from ctx.Services if possible. //
        public DiscordShardedClient Client { get; set; }
        public static Bot? Instance { get; private set; }
        public static string DefaultCommandPrefix { get; } = "s!";
        public SilkDbContext SilkDBContext { get; }

        public CommandsNextConfiguration? Commands { get; private set; }

        private readonly IServiceProvider _services;
        private readonly ILogger<Bot> _logger;
        private readonly BotExceptionHelper _exceptionHelper;
        private readonly BotEventSubscriber _eventSubscriber;
        private readonly Stopwatch _sw = new();


        public Bot(IServiceProvider services, DiscordShardedClient client, ILogger<Bot> logger, BotExceptionHelper exceptionHelper, BotEventSubscriber eventSubscriber, IDbContextFactory<SilkDbContext> dbFactory)
        {
            _sw.Start();
            _services = services;
            _logger = logger;
            _exceptionHelper = exceptionHelper;
            _eventSubscriber = eventSubscriber;
            
            SilkDBContext = dbFactory.CreateDbContext();

            try { _ = SilkDBContext.Guilds.FirstOrDefaultAsync(); }
            catch
            {
                SilkDBContext.Database.MigrateAsync().GetAwaiter().GetResult();
                _logger.LogInformation("Database not set up! Migrating...");
            }
            Instance = this;
            Client = client;
        }
        
        private void InitializeCommands()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            IReadOnlyDictionary<int, CommandsNextExtension> cNext = Client.GetCommandsNextAsync().GetAwaiter().GetResult();
            CommandsNextExtension[] extension = cNext.Select(c => c.Value).ToArray();

            var sw = Stopwatch.StartNew();
            foreach (CommandsNextExtension c in extension)
                c.RegisterCommands(asm);
            sw.Stop();
            
            _logger.LogDebug($"Registered commands for {Client.ShardClients.Count} shard(s) in {sw.ElapsedMilliseconds} ms.");
        }

        private async Task InitializeClientAsync()
        {
            Commands = new CommandsNextConfiguration
            {
                UseDefaultCommandHandler = false,
                Services = _services,
                IgnoreExtraArguments = true,
                
            };
            Client.Ready += async (c, _) => _logger.LogInformation($"Recieved OP 10 - HELLO from Discord on shard {c.ShardId + 1}!");


            await Client.UseCommandsNextAsync(Commands);
            await _exceptionHelper.SubscribeToEventsAsync();
            _eventSubscriber.SubscribeToEvents();
            InitializeCommands();
            
            await Client.UseInteractivityAsync(new()
            {
                PaginationBehaviour = PaginationBehaviour.WrapAround,
                PaginationDeletion = PaginationDeletion.DeleteMessage,
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromMinutes(1)
            });

            
            IReadOnlyDictionary<int, CommandsNextExtension>? cmdNext = await Client.GetCommandsNextAsync();
            CommandsNextExtension[] cnextExtensions = cmdNext.Select(c => c.Value).ToArray();
            var memberConverter = new MemberConverter();
            
            foreach (CommandsNextExtension extension in cnextExtensions)
            {
                extension.SetHelpFormatter<HelpFormatter>();
                extension.RegisterConverter(memberConverter);
            }
            await Client.StartAsync();
            
            double startupDt = DateTime.Now.Subtract(Program.Startup).TotalMilliseconds;
            _logger.LogInformation($"Startup time: {startupDt:N0} ms.");
        }

        public async Task StartAsync(CancellationToken cancellationToken) => await InitializeClientAsync();

        public async Task StopAsync(CancellationToken cancellationToken) => await Client.StopAsync();

    }
}