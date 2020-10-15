namespace SilkBot
{
    #region Usings
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using DSharpPlus.Interactivity;
    using DSharpPlus.Interactivity.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using SilkBot.Commands.Bot;
    using SilkBot.Commands.Moderation.Utilities;
    using SilkBot.Models;
    using SilkBot.Server;
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
        public InteractivityConfiguration Interactivity { get; }
        public static DateTime StartupTime { get; } = DateTime.Now;
        public static string SilkDefaultCommandPrefix { get; } = "!";
        public static Stopwatch CommandTimer { get; } = new Stopwatch();
        public SilkDbContext SilkDBContext { get; } = new SilkDbContext();
        public TimerBatcher Timer { get; } = new TimerBatcher(new ActionDispatcher());
        public IServiceProvider Services { get; private set; }
        public CommandsNextConfiguration Commands { get; private set; }
        
        #endregion

        private readonly Stopwatch sw = new Stopwatch();

        private Bot() => sw.Start();
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
            AddServices();
            await InitializeClientAsync();
            
            await Task.Delay(-1);
        }

        private void AddServices()
        {
            var emojiService = new DiscordEmojiCreationService();
            Services = new ServiceCollection()
                .AddSingleton(emojiService)
                .AddDbContextFactory<SilkDbContext>(d => _ = d, ServiceLifetime.Transient)
                .BuildServiceProvider();
            
            Commands = new CommandsNextConfiguration { EnableDefaultHelp = false, UseDefaultCommandHandler = false, Services = Services };
        }

        private async Task OnGuildAvailable(DiscordClient c, GuildCreateEventArgs eventArgs)
        {
            _ = Task.Run(async () =>
            {
                var db = Services.GetService<IDbContextFactory<SilkDbContext>>().CreateDbContext();
                var guild = await GetOrCreateGuildAsync(eventArgs.Guild.Id);
                if (!db.Guilds.Contains(guild)) db.Guilds.Add(guild);
                await CacheStaffMembers(guild, eventArgs.Guild.Members.Values);
            });
            //TODO: Fix Logger
            //eventArgs.Client.DebugLogger.LogMessage(LogLevel.Info, "Silk!", $"Guild available: {eventArgs.Guild.Name}", DateTime.Now);
        }

        public async Task CacheStaffMembers(Guild guild, IEnumerable<DiscordMember> members)
        {
            var staffMembers = members
                .Where(member => member.HasPermission(Permissions.KickMembers) && !member.IsBot)
                .Select(staffMember => new DiscordUserInfo {Guild = guild, UserId = staffMember.Id, Flags = UserFlag.Staff});


            guild.DiscordUserInfos.AddRange(staffMembers);
            await SilkDBContext.SaveChangesAsync();
        }

        public async Task<Guild> GetOrCreateGuildAsync(ulong guildId)
        {
            var db = Services.GetService<IDbContextFactory<SilkDbContext>>().CreateDbContext();
            var guild = await db.Guilds.FirstOrDefaultAsync(g => g.DiscordGuildId == guildId);
            
            if (guild != null)
            {
                return guild;
            }

            guild = new Guild { DiscordGuildId = guildId, Prefix = SilkDefaultCommandPrefix };
            return guild;
        }

        private Task OnReady(DiscordClient c, ReadyEventArgs e)
        {
            //TODO: Fix Logger
            //e.Client.DebugLogger.LogMessage(LogLevel.Info, "Silk!", "Ready to process events.", DateTime.Now);
            return Task.CompletedTask;
        }

        private void RegisterCommands() => Client.GetCommandsNext().RegisterCommands(Assembly.GetExecutingAssembly());

        private async Task InitializeClientAsync()
        {
            var token = File.ReadAllText("./Token.txt");
            var config = new DiscordConfiguration
            {
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Information,
                Token = token,
                TokenType = TokenType.Bot,
                
            };

            Client = new DiscordClient(config);

            Client.UseInteractivity(new InteractivityConfiguration { PaginationBehaviour = PaginationBehaviour.WrapAround, Timeout = TimeSpan.FromMinutes(1)});

            Client.UseCommandsNext(Commands);

            // Register database context and apply newest migration if not already done.

            RegisterCommands();
            HelpCache.Initialize();
            

            Client.Ready += OnReady;
            Client.GuildAvailable += OnGuildAvailable;

            await Client.ConnectAsync();
            CreateHandlers();

            sw.Stop();
            Colorful.Console.WriteLine($"Startup time: {(sw.ElapsedMilliseconds / 1000d):F2} seconds", Color.CornflowerBlue);
        }

        private void CreateHandlers()
        {
            Client.Ready += OnReady;
            Client.GuildAvailable += OnGuildAvailable;
            new MessageDeletionHandler(Client);
            new MessageEditHandler(Client);
            new GuildJoinHandler();
            new MessageCreationHandler();
            new GuildMemberCountChangeHandler(Client);
        }
        #endregion
    }
}
