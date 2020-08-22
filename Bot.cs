using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using Newtonsoft.Json;
using SilkBot.Commands.Economy;
using SilkBot.Commands.Moderation.Utilities;
using SilkBot.ServerConfigurations;
using SilkBot.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace SilkBot
{
    public class Bot
    {

        public static Bot Instance { get; } = new Bot();
        public DataStorageContainer Data { get => _data; }
        private DataStorageContainer _data = new DataStorageContainer();
        public static DiscordEconomicUsersData EconomicUsers { get; set; } = new DiscordEconomicUsersData();


        [JsonProperty(PropertyName = "Guild Prefixes")]
        public static Dictionary<ulong, string> GuildPrefixes { get; set; }

        public DiscordClient Client { get; set; }
        public CommandsNextConfiguration Commands { get; } = new CommandsNextConfiguration { CaseSensitive = false, EnableDefaultHelp = false, EnableMentionPrefix = true };

        public InteractivityConfiguration Interactivity { get; }

        public TimedActionHelper Timer { get; } = new TimedActionHelper();
 
        private Bot() { }
       

        public async Task RunBotAsync()
        {
            await InitializeClient();
            await Task.Delay(-1);

        }

        private Task OnGuildJoin(GuildCreateEventArgs e)
        {
            

            return Task.CompletedTask;
        }

        private Task OnMessageCreate(MessageCreateEventArgs e)
        {
            if (e.Author.IsBot) return Task.FromResult(-1);
            var prefix = GuildPrefixes[e.Guild.Id];
            var prefixPos = e.Message.GetStringPrefixLength(prefix);
            if (prefixPos < 0) return Task.FromResult(-1);
            var pfx = e.Message.Content.Substring(0, prefixPos);
            var cnt = e.Message.Content.Substring(prefixPos);

            var cmd = Client.GetCommandsNext().FindCommand(cnt, out var args);
            var ctx = Client.GetCommandsNext().CreateContext(e.Message, pfx, cmd, args);
            if (cmd is null) return Task.FromResult(-1);


            Task.Run(async () => await Client.GetCommandsNext().ExecuteCommandAsync(ctx));
            return Task.CompletedTask;
        }

        private Task OnCommandErrored(CommandErrorEventArgs e)
        {
            switch (e.Exception)
            {
                case InsufficientFundsException _:
                    e.Context.Channel.SendMessageAsync(e.Exception.Message);
                    break;
                default:
                    Client.DebugLogger.LogMessage(LogLevel.Error, "Silk!", e.Exception.Message, DateTime.Now);
                    break;
            }
            return Task.CompletedTask;
        }

        private Task OnGuildAvailable(GuildCreateEventArgs e)
        {
            if (!ServerConfigurationManager.LocalConfiguration.ContainsKey(e.Guild.Id))
                ServerConfigurationManager.Instance.GenerateConfigurationFromIdAsync(e.Guild.Id).GetAwaiter();
            if(!GuildPrefixes.ContainsKey(e.Guild.Id))
                GuildPrefixes.Add(e.Guild.Id, "!"); 
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "Silk!", $"Guild available: {e.Guild.Name}", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task OnReady(ReadyEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "Silk!", "Ready to process events.", DateTime.Now);
            return Task.CompletedTask;
        }

        private void RegisterCommands() => Client.GetCommandsNext().RegisterCommands(Assembly.GetExecutingAssembly());


        private async Task InitializeClient()
        {
            var Token = File.ReadAllText("./Token.txt");
            var config = new DiscordConfiguration
            {
                AutoReconnect = true,
                LogLevel = LogLevel.Warning | LogLevel.Info,
                Token = Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
            };

            Client = new DiscordClient(config);

            Client.UseInteractivity(new InteractivityConfiguration { PaginationBehaviour = PaginationBehaviour.WrapAround, Timeout = TimeSpan.FromMinutes(1) });


            Client.UseCommandsNext(Commands);

            RegisterCommands();

            HelpCache.Initialize(DiscordColor.Azure);
            Data.PopulateDataOnApplicationLoad();
            //All these handlers do is subscibe to the bot's appropriate event, and do something, hence not assigning a variable to it.
            new MessageDeletionHandler(ref _data, Client);
            new MessageEditHandler(ref _data, Client);

            var prefixData = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkBot", "Configs", "prefixes.gconfig"));
            GuildPrefixes = JsonConvert.DeserializeObject<Dictionary<ulong, string>>(prefixData);

            Client.Ready += OnReady;
            Client.GuildAvailable += OnGuildAvailable;
            Client.GuildCreated += OnGuildJoin;
            Client.GetCommandsNext().CommandErrored += OnCommandErrored;
            Client.MessageCreated += OnMessageCreate;
            Client.MessageDeleted += async (e) => await Task.CompletedTask;
            Client.GuildDownloadCompleted += async (e) => 
            {
                Client.DebugLogger.LogMessage(LogLevel.Info, "Silk!", $"Availble guilds: {e.Guilds.Count}", DateTime.Now);
                await Data.FetchGuildInfo(Client.Guilds.Values);
            };

            await Client.ConnectAsync();

        }
    }
}
