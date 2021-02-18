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
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Silk.Core.Database;
using Silk.Core.EventHandlers;
using Silk.Core.EventHandlers.MemberAdded;
using Silk.Core.EventHandlers.MessageAdded;
using Silk.Core.EventHandlers.MessageAdded.AutoMod;
using Silk.Core.Utilities.Bot;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Extensions;

namespace Silk.Core
{

    public class Bot : IHostedService
    {
        public DiscordShardedClient Client { get; set; }
        public static Bot? Instance { get; private set; }
        public static string DefaultCommandPrefix { get; } = "s!";
        public SilkDbContext SilkDBContext { get; }

        public CommandsNextConfiguration? Commands { get; private set; }

        private readonly IMediator _mediator;
        private readonly IServiceProvider _services;
        private readonly ILogger<Bot> _logger;
        private readonly BotExceptionHandler _exceptionHandler;
        private readonly Stopwatch _sw = new();


        public Bot(            
            IMediator mediator,
            ILogger<Bot> logger, 
            IServiceProvider services, 
            DiscordShardedClient client, 
            BotExceptionHandler exceptionHandler, 
            IDbContextFactory<SilkDbContext> dbFactory)
        {
            _sw.Start();
            _services = services;
            _logger = logger;
            _exceptionHandler = exceptionHandler;
            _mediator = mediator;

            SilkDBContext = dbFactory.CreateDbContext();

            try { _ = SilkDBContext.Guilds.FirstOrDefault(); }
            catch
            {
                _logger.LogInformation("Database not set up! Migrating...");
                SilkDBContext.Database.Migrate();
            }
            Instance = this;
            Client = client;
        }
        private void InitializeServices() { }

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

            await Client.UseCommandsNextAsync(Commands);
            await _exceptionHandler.SubscribeToEventsAsync();

            InitializeCommands();
            InitializeServices();
            SubscribeToEvents();

            await Client.UseInteractivityAsync(new()
            {
                PaginationBehaviour = PaginationBehaviour.WrapAround,
                PaginationDeletion = PaginationDeletion.DeleteMessage,
                PollBehaviour = PollBehaviour.DeleteEmojis,
                Timeout = TimeSpan.FromMinutes(1)
            });


            IReadOnlyDictionary<int, CommandsNextExtension>? cmdNext = await Client.GetCommandsNextAsync();
            CommandsNextExtension[] cnextExtensions = cmdNext.Select(c => c.Value).ToArray();


            foreach (CommandsNextExtension extension in cnextExtensions)
            {
                extension.SetHelpFormatter<HelpFormatter>();
                extension.RegisterConverter(new MemberConverter());
            }

            _logger.LogInformation($"Services + Commands initialized in: {DateTime.Now.Subtract(Program.Startup).TotalMilliseconds:N0} ms");
            await Client.StartAsync();

            // Client.StartAsync() returns as soon as all shards are ready, which means we log before
            // The client is *actually* ready.
            while (!GuildAddedHandler.StartupCompleted) { }
            _logger.LogInformation($"All shards initialized in: {DateTime.Now.Subtract(Program.Startup).TotalMilliseconds:N0} ms");
        }


        // Cluserfuck of a method. I know. //
        private void SubscribeToEvents()
        {
            _logger.LogDebug("Subscribing to events");
            
            Client.MessageCreated += _services.Get<MessageAddedHandler>().Commands;
            _logger.LogTrace("Subscribed to:" + " MessageAddedHelper/Commands".PadLeft(40));
            Client.MessageCreated += _services.Get<MessageAddedHandler>().Tickets;
            _logger.LogTrace("Subscribed to:" + " MessageAddedHelper/Tickets".PadLeft(40));
            Client.MessageCreated += _services.Get<AutoModInviteHandler>().MessageAddInvites;
            _logger.LogTrace("Subscribed to:" + " AutoMod/CheckAddInvites".PadLeft(40));
            Client.MessageUpdated += _services.Get<AutoModInviteHandler>().MessageEditInvites;
            _logger.LogTrace("Subscribed to:" + " AutoMod/CheckEditInvites".PadLeft(40));
            Client.MessageDeleted += _services.Get<MessageRemovedHandler>().MessageRemoved;
            _logger.LogTrace("Subscribed to:" + " MessageRemovedHelper/MessageRemoved".PadLeft(40));
            Client.GuildMemberAdded += _services.Get<MemberAddedHandler>().OnMemberAdded;
            _logger.LogTrace("Subscribed to:" + " MemberAddedHandler/MemberAdded".PadLeft(40));
            Client.GuildMemberRemoved += _services.Get<MemberRemovedHandler>().OnMemberRemoved;
            _logger.LogTrace("Subscribed to:" + " MemberRemovedHelper/MemberRemoved".PadLeft(40));
            Client.GuildCreated += _services.Get<GuildAddedHandler>().SendThankYouMessage;
            _logger.LogTrace("Subscribed to:" + " GuildAddedHelper/SendWelcomeMessage".PadLeft(40));
            Client.GuildAvailable += _services.Get<GuildAddedHandler>().OnGuildAvailable;
            _logger.LogTrace("Subscribed to:" + " GuildAddedHelper/GuildAvailable".PadLeft(40));
            Client.GuildDownloadCompleted += _services.Get<GuildAddedHandler>().OnGuildDownloadComplete;
            _logger.LogTrace("Subscribed to:" + "  GuildAddedHelper/GuildDownloadComplete");
            Client.GuildMemberUpdated += _services.Get<RoleAddedHandler>().CheckStaffRole;
            _logger.LogTrace("Subscribed to:" + " RoleAddedHelper/CheckForStaffRole".PadLeft(40));
            _logger.LogInformation("Subscribed to all events!");
        }


        public async Task StartAsync(CancellationToken cancellationToken) => await InitializeClientAsync();

        public async Task StopAsync(CancellationToken cancellationToken) => await Client.StopAsync();

    }
}