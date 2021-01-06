using System;
using System.Diagnostics;
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
using Silk.Core.Database;
using Silk.Core.Services;
using Silk.Core.Tools.EventHelpers;
using Silk.Core.Utilities;
using Silk.Extensions;

namespace Silk.Core
{

    public class Bot : IHostedService
    {
        //TODO: Fix all these usages, because they should be pulling from ctx.Services if possible. //
        public DiscordShardedClient Client          { get; set; }
        public static Bot? Instance                 { get; private set; }
        public static string DefaultCommandPrefix   { get; } = "s!";
        public static Stopwatch CommandTimer        { get; } = new();
        public SilkDbContext SilkDBContext          { get; private set; }
        
        public CommandsNextConfiguration? Commands  { get; private set; }

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
            Instance = this;
            Client = client;
        }

        private void InitializeCommands()
        {
            var sw = Stopwatch.StartNew();
            
            foreach (DiscordClient shard in Client.ShardClients.Values)
                shard.GetCommandsNext().RegisterCommands(Assembly.GetExecutingAssembly());
            
            sw.Stop();
            _logger.LogDebug($"Registered commands for {Client.ShardClients.Count} shards in {sw.ElapsedMilliseconds} ms.");
        }

        private async Task InitializeClientAsync()
        {
            Commands = new CommandsNextConfiguration
            {
                UseDefaultCommandHandler = false,
                Services = _services,
                IgnoreExtraArguments = true
            };
            Client.Ready += async (_, _) => _logger.LogInformation($"Recieved OP 10 - HELLO from Discord on shard 1!");
            
            
            await Client.UseCommandsNextAsync(Commands);
            await _exceptionHelper.SubscribeToEventsAsync();
            _eventSubscriber.SubscribeToEvents();
            InitializeCommands();
            await Client.StartAsync();
            await Client.UseInteractivityAsync(new InteractivityConfiguration
            {
                PaginationBehaviour = PaginationBehaviour.WrapAround,
                PaginationDeletion = PaginationDeletion.DeleteMessage,
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromMinutes(1),
            });
            
            
            
            var cmdNext = await Client.GetCommandsNextAsync();
            foreach (CommandsNextExtension c in cmdNext.Values)
            {
                c.SetHelpFormatter<HelpFormatter>(); 
                c.RegisterConverter(new MemberConverter());
            }
           
            _logger.LogInformation($"Startup time: {DateTime.Now.Subtract(Program.Startup).Seconds} seconds.");
            
            
        }

        public async Task StartAsync(CancellationToken cancellationToken) => await InitializeClientAsync();

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Client.StopAsync();
            _services.Get<InfractionService>().StopInfractionThread();
            
        }

    }
}