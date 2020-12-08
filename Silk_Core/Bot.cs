#region Usings

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SilkBot.Commands;
using SilkBot.Commands.Bot;
using SilkBot.Database;
using SilkBot.Services;
using SilkBot.Utilities;

namespace SilkBot
{

    #endregion

    public class Bot : IHostedService
    {
        #region Props
        //TODO: Fix all these usages, because they should be pulling from ctx.Services if possible. //
        public DiscordShardedClient Client          { get; set; }
        public static Bot? Instance                 { get; private set; }
        public static string DefaultCommandPrefix   { get; } = "s!";
        public static Stopwatch CommandTimer        { get; } = new();
        public SilkDbContext SilkDBContext          { get; private set; }
        
        public CommandsNextConfiguration? Commands  { get; private set; }

        #endregion

        private readonly IServiceProvider _services;
        private readonly ILogger<Bot> _logger;
        private readonly BotEventHelper _eventHelper;
        private readonly PrefixCacheService _prefixService;
        private readonly Stopwatch _sw = new();
        
        
        public Bot(IServiceProvider services, DiscordShardedClient client,
            ILogger<Bot> logger, BotEventHelper eventHelper, PrefixCacheService prefixService,
            MessageCreationHandler msgHandler, CommandProcessorModule commandProcessor, IDbContextFactory<SilkDbContext> dbFactory)
        {
            
            _sw.Start();
            _services = services;
            _logger = logger;
            _eventHelper = eventHelper;
            _prefixService = prefixService;
            client.MessageCreated += msgHandler.OnMessageCreate;
            client.MessageCreated += commandProcessor.OnMessageCreate;
            SilkDBContext = dbFactory.CreateDbContext();
            SilkDBContext.Database.Migrate();
            Instance = this;
            Client = client;
        }

        #region Methods
        
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
            
            await Client.StartAsync();
            
            await Client.UseCommandsNextAsync(Commands);
            InitializeCommands();
            
            await Client.UseInteractivityAsync(new InteractivityConfiguration
            {
                PaginationBehaviour = PaginationBehaviour.WrapAround,
                PaginationDeletion = PaginationDeletion.DeleteMessage,
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromMinutes(1),
            });
            
            _eventHelper.CreateHandlers();
            
            var cmdNext = await Client.GetCommandsNextAsync();
            foreach (CommandsNextExtension c in cmdNext.Values) c.SetHelpFormatter<HelpFormatter>();
            foreach (CommandsNextExtension c in cmdNext.Values) c.RegisterConverter(new MemberConverter());
            _logger.LogInformation("Client Initialized.");
            _logger.LogInformation($"Startup time: {DateTime.Now.Subtract(Program.Startup).Seconds} seconds.");
            
            Client.Ready += async (c, e) => _logger.LogInformation("Client ready to proccess commands.");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await InitializeClientAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Client.StopAsync();
        }

        #endregion
    }
}