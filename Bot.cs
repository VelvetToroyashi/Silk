using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using SilkBot.Commands.Economy;
using SilkBot.ServerConfigurations;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace SilkBot
{
    public class Bot
    {

        public static Bot Instance { get; } = new Bot();

        public DiscordClient Client { get; set; }
        public CommandsNextConfiguration Commands { get; } 
        public InteractivityConfiguration Interactivity { get; } 
        
        
        private double startupTime;

         
        private Bot() { }


        public async Task RunBotAsync()
        {
            InitializeClient();
            HelpCache.Initialize(DiscordColor.Azure);
            ServerConfigurationManager.Instance.LoadServerConfigs();
            

            Client.Ready += OnReadyEvent;
            Client.GuildAvailable += OnGuildAvailable;
            Client.GetCommandsNext().CommandErrored += OnCommandErrored;

             
            await Client.ConnectAsync();
            await Task.Delay(-1);

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

        private Task OnReadyEvent(DSharpPlus.EventArgs.ReadyEventArgs e)
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

            var cnc = new CommandsNextConfiguration { CaseSensitive = false, EnableDefaultHelp = false, EnableMentionPrefix = false, StringPrefixes = new string[] { "!" } };

            Client.UseCommandsNext(cnc);

            RegisterCommands();
        }

    }
}
