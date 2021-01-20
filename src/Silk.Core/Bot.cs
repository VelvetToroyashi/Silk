using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
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
        public static Stopwatch CommandTimer { get; } = new();
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
            
            try { _ = SilkDBContext.Guilds.FirstOrDefault(); }
            catch (PostgresException) { SilkDBContext.Database.MigrateAsync().GetAwaiter().GetResult(); }
            
            Instance = this;
            Client = client;
        }

        private void InitializeCommands()
        {
            var sw = Stopwatch.StartNew();
            Assembly asm = Assembly.GetExecutingAssembly();
            IReadOnlyDictionary<int, CommandsNextExtension> cNext = Client.GetCommandsNextAsync().GetAwaiter().GetResult();
            CommandsNextExtension[] extension = cNext.Select(c => c.Value).ToArray();
            
            
            // The compiler can unwrap(?) this I believe. Either way for > foreach anyway. //
            for(var i = 0; i < extension.Length; i++) { extension[i].RegisterCommands(asm); }

            sw.Stop();
            _logger.LogDebug($"Registered commands for {Client.ShardClients.Count} shard(s) in {sw.ElapsedMilliseconds} ms.");
        }

        private async Task InitializeClientAsync()
        {
            Commands = new CommandsNextConfiguration
            {
                UseDefaultCommandHandler = false,
                Services = _services,
                IgnoreExtraArguments = true
            };
            Client.Ready += async (_, _) => _logger.LogInformation("Recieved OP 10 - HELLO from Discord on shard 1!");
            await Client.StartAsync();

            await Client.UseCommandsNextAsync(Commands);
            await _exceptionHelper.SubscribeToEventsAsync();
            _eventSubscriber.SubscribeToEvents();
            InitializeCommands();
            
            await Client.UseInteractivityAsync(new InteractivityConfiguration
            {
                PaginationBehaviour = PaginationBehaviour.WrapAround,
                PaginationDeletion = PaginationDeletion.DeleteMessage,
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromMinutes(1),
            });

            
            IReadOnlyDictionary<int, CommandsNextExtension>? cmdNext = await Client.GetCommandsNextAsync();
            CommandsNextExtension[] cmd = cmdNext.Select(c => c.Value).ToArray();
            var memberConverter = new MemberConverter();
            
            for (int i = 0; i < cmd.Length; i++)
            {
                cmd[i].SetHelpFormatter<HelpFormatter>();
                cmd[i].RegisterConverter(memberConverter);
            }

            _logger.LogInformation($"Startup time: {DateTime.Now.Subtract(Program.Startup).Milliseconds} ms.");
            
        }

        public async Task StartAsync(CancellationToken cancellationToken) => await InitializeClientAsync();

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Client.StopAsync();
        }

    }
}