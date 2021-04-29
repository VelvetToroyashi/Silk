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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Silk.Core.Data;
using Silk.Core.Discord.EventHandlers;
using Silk.Core.Discord.EventHandlers.MemberAdded;
using Silk.Core.Discord.EventHandlers.MessageAdded.AutoMod;
using Silk.Core.Discord.EventHandlers.Notifications;
using Silk.Core.Discord.Types;
using Silk.Core.Discord.Utilities.Bot;
using Silk.Core.Discord.Utilities.HelpFormatter;
using Silk.Extensions;
using Silk.Shared.Abstractions.DSharpPlus.Concrete;

namespace Silk.Core.Discord
{
    //Lorum Ipsum, or something.
    public class Bot : IHostedService
    {
        public static BotState State { get; set; } = BotState.Starting;

        private readonly BotExceptionHandler _exceptionHandler;
        private readonly ILogger<Bot> _logger;

        private readonly IMediator _mediator;
        private readonly IServiceProvider _services;
        private readonly Stopwatch _sw = new();

        private CommandsNextConfiguration? _commands;


        public Bot(
            IMediator mediator,
            ILogger<Bot> logger,
            IServiceProvider services,
            DiscordShardedClient client,
            BotExceptionHandler exceptionHandler,
            IDbContextFactory<GuildContext> dbFactory)
        {
            _sw.Start();
            _services = services;
            _logger = logger;
            _exceptionHandler = exceptionHandler;
            _mediator = mediator;

            try
            {
                _logger.LogInformation("Migrating core database!");
                dbFactory.CreateDbContext().Database.Migrate();
            }
            catch (PostgresException)
            {
                /* Ignored. */
            }

            Instance = this;
            Client = client;
        }
        public DiscordShardedClient Client { get; set; }
        public static Bot? Instance { get; private set; }
        public static string DefaultCommandPrefix { get; } = "s!";
        private void InitializeServices()
        {
            _ = _services.GetRequiredService<AntiInviteCore>();
            // Logger has to be setup in that class before it can be used properly. //
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

            _logger.LogDebug("Registered commands for {Shards} shard(s) in {Time} ms", Client.ShardClients.Count, sw.ElapsedMilliseconds);
        }

        private async Task InitializeClientAsync()
        {
            _commands = new CommandsNextConfiguration
            {
                UseDefaultCommandHandler = false,
                Services = _services,
                IgnoreExtraArguments = true
            };

            await Client.UseCommandsNextAsync(_commands);
            InitializeCommands();
            InitializeServices();
            SubscribeToEvents();

            await _exceptionHandler.SubscribeToEventsAsync();

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

            _logger.LogInformation("Bot initialized in: {Time} ms", DateTime.Now.Subtract(Program.Startup).TotalMilliseconds.ToString("N0"));
            await Client.StartAsync();

            // Client.StartAsync() returns as soon as all shards are ready, which means we log before
            // The client is *actually* ready.
            while (!GuildAddedHandler.StartupCompleted) { }
            _logger.LogInformation("All shards initialized in: {Time} ms", DateTime.Now.Subtract(Program.Startup).TotalMilliseconds.ToString("N0"));
        }

        // Clusterfuck of a method. I know. //
        //TODO: Change this to use MediatR & INotification<T>/INotificationHandler<T>
        private void SubscribeToEvents()
        {
            Client.MessageCreated += async (c, e) => { _ = _mediator.Publish(new MessageCreated(c, e.Message!)); };
            Client.MessageUpdated += async (c, e) => { _ = _mediator.Publish(new MessageEdited(c, e)); };
            //Client.MessageCreated += _services.Get<AutoModInviteHandler>().MessageAddInvites;

            Client.MessageDeleted += _services.Get<MessageRemovedHandler>()!.MessageRemoved;

            Client.GuildMemberAdded += _services.Get<MemberAddedHandler>()!.OnMemberAdded;
            Client.GuildCreated += _services.Get<GuildAddedHandler>()!.SendThankYouMessage;
            Client.GuildAvailable += _services.Get<GuildAddedHandler>()!.OnGuildAvailable;
            Client.GuildCreated += _services.Get<GuildAddedHandler>()!.OnGuildAvailable;
            Client.GuildDownloadCompleted += _services.Get<GuildAddedHandler>()!.OnGuildDownloadComplete;
            Client.GuildMemberUpdated += _services.Get<RoleAddedHandler>()!.CheckStaffRole;

            Client.GuildDownloadCompleted += async (_, _) =>
            {
                foreach (var g in Client.ShardClients.Values.SelectMany(c => c.Guilds.Values))
                    _ = (Guild) g!;
            };
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await InitializeClientAsync();

        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Client.StopAsync();
            _logger.LogInformation("Disconnected from Discord Gateway");
        }
    }
}