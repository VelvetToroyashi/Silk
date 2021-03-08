using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.EventArgs;
using Humanizer;
using Humanizer.Localisation;
using Microsoft.Extensions.Logging;
using Serilog;
using Silk.Extensions;

namespace Silk.Core.Utilities.Bot
{

    public class BotExceptionHandler
    {
        private readonly ILogger<BotExceptionHandler> _logger;
        private readonly DiscordShardedClient _client;
        public BotExceptionHandler(ILogger<BotExceptionHandler> logger, DiscordShardedClient client)
        {
            _logger = logger;
            _client = client;
            _client.ClientErrored += OnClientErrored;
            _client.SocketClosed += OnSocketErrored;
            //I know this is terrible, but, piss off :p //
            
        }


        private async Task OnCommandErrored(CommandsNextExtension c, CommandErrorEventArgs e)
        {
            
        }

        private async Task OnClientErrored(DiscordClient c, ClientErrorEventArgs e)
        {
            if (e.Exception.Message.Contains("event"))
            {
                _logger.LogWarning("[{Event}] Timed out!", e.EventName);
            }
            else
            {
                _logger.LogWarning("Something went wrong! {Exception}", e.Exception);
            }
        }
        private async Task OnSocketErrored(DiscordClient c, SocketCloseEventArgs e)
        {
            if (e.CloseCode is 4014)  
                 _logger.LogCritical("Missing intents! Enable them on the developer dashboard (discord.com/developers/applications/{AppId})", _client.CurrentApplication.Id);
        }
        public static async Task SendHelpAsync(DiscordClient c, string commandName, CommandContext originalContext)
        {
            CommandsNextExtension? cnext = c.GetCommandsNext();
            Command? cmd = cnext.RegisteredCommands["help"];
            CommandContext? ctx = cnext.CreateContext(originalContext.Message, null, cmd, commandName);
            await cnext.ExecuteCommandAsync(ctx);
        }

        public async Task SubscribeToEventsAsync()
        {
            TaskScheduler.UnobservedTaskException += async (_, e) => _logger.LogError("Task Scheduler caught an unobserved exception: {Exception}",  e.Exception);
            
            IEnumerable<CommandsNextExtension?> commandsNext = (await _client.GetCommandsNextAsync()).Values;

            foreach (CommandsNextExtension? c in commandsNext)
                c!.CommandErrored += OnCommandErrored;
        }
    }
}