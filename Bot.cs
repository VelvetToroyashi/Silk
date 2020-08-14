using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using Newtonsoft.Json;
using SilkBot.Commands.Economy;
using SilkBot.ServerConfigurations;
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
        public static GlobalUserConfiguration GlobalConfig { get; set; } = new GlobalUserConfiguration();
        [JsonProperty(PropertyName = "Guild Prefixes")]
        public static Dictionary<ulong, string> GuildPrefixes { get; set; }

        public DiscordClient Client { get; set; }
        public CommandsNextConfiguration Commands { get; } = new CommandsNextConfiguration { CaseSensitive = false, EnableDefaultHelp = false, EnableMentionPrefix = true };

        public InteractivityConfiguration Interactivity { get; }

 
        private Bot() { }
       

        public async Task RunBotAsync()
        {
            InitializeClient();
            HelpCache.Initialize(DiscordColor.Azure);
            ServerConfigurationManager.Instance.LoadServerConfigs();


            Client.Ready += OnReady;
            Client.GuildAvailable += OnGuildAvailable;
            Client.GuildCreated += OnGuildJoin;
            Client.GetCommandsNext().CommandErrored += OnCommandErrored;
            Client.MessageCreated += OnMessageCreate;


            await Client.ConnectAsync();

            await Task.Delay(-1);

        }

        private Task OnGuildJoin(GuildCreateEventArgs e)
        {
            GuildPrefixes.Add(e.Guild.Id, "!");
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
            if (cmd == null)
            {
                return Task.FromResult(-1);
            }

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

        private Task OnGuildAvailable(DSharpPlus.EventArgs.GuildCreateEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "Silk!", $"Guild available: {e.Guild.Name}", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task OnReady(DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "Silk!", "Ready to process events.", DateTime.Now);
            return Task.CompletedTask;
        }

        private void RegisterCommands() => Client.GetCommandsNext().RegisterCommands(Assembly.GetExecutingAssembly());


        private void InitializeClient()
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

            Client.UseInteractivity(new InteractivityConfiguration { PaginationBehaviour = PaginationBehaviour.Ignore, Timeout = TimeSpan.FromMinutes(2) });


            Client.UseCommandsNext(Commands);

            RegisterCommands();
        }

    }
}
