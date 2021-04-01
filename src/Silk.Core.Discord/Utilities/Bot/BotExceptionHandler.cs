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
using Silk.Core.Discord.EventHandlers.MessageAdded;
using Silk.Extensions;

namespace Silk.Core.Discord.Utilities.Bot
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
            CommandHandler.ParserErrored += OnParserErrored;
        }
        private void OnParserErrored(string command, Exception e)
        {
            // ReSharper disable once ExceptionPassedAsTemplateArgumentProblem
            _logger.LogWarning("Couldn't find that command!: {CommandName}, Exception: {Exception}", command, e.InnerException ?? e);
        }


        private async Task OnCommandErrored(CommandsNextExtension c, CommandErrorEventArgs e)
        {
            Task task = e.Exception switch
            {
                ChecksFailedException f => ShowChecksFailedMessage(e, c.Client, f),

                //Command parsing exceptions //
                ArgumentException {Message: "Could not find a suitable overload for the command."}
                    => SendHelpAsync(c.Client, e.Command.QualifiedName, e.Context),
                InvalidOperationException {Message: "No matching subcommands were found, and this group is not executable."}
                    => SendHelpAsync(c.Client, e.Command.QualifiedName, e.Context),

                _ => Task.CompletedTask
            };
            await task;

            Log.Logger.ForContext(e.Command.Module.ModuleType).Warning(e.Exception, "Exception thrown!");
            //_logger.LogWarning(e.Exception.InnerException ?? e.Exception , "A command threw an exception! Command: {CommandName}", e.Command.Name);
        }

        private async Task ShowChecksFailedMessage(CommandErrorEventArgs e, DiscordClient c, ChecksFailedException cf)
        {
            switch (cf.FailedChecks[0])
            {
                case RequireOwnerAttribute:
                    string owner = c.CurrentApplication.Owners.Select(o => $"{o.Username}#{o.Discriminator}").Join(", ");
                    await e.Context.RespondAsync($"My owners consist of: {owner}. {cf.Context.User.Username}#{cf.Context.User.Discriminator} doesn't look like any of those names!");
                    break;
                case RequireNsfwAttribute:
                    await e.Context.RespondAsync("Hot, but this channel isn't that spicy! (Mark it as NSFW and I'll budge ;3)");
                    break;
                case RequireFlagAttribute f:
                    await e.Context.RespondAsync($"Heh. You need to be {f.RequisiteUserFlag.Humanize(LetterCasing.Title)} for that.");
                    break;
                case CooldownAttribute cd:
                    await e.Context.RespondAsync($"Sorry, but this command has a cooldown! You can use it {cd.MaxUses} time(s) every {cd.Reset.Humanize(2, minUnit: TimeUnit.Second)}!");
                    break;
                case RequireUserPermissionsAttribute p:
                    await e.Context.RespondAsync($"You need to have permission to {p.Permissions.Humanize(LetterCasing.Title)} to run this!");
                    break;
                case RequireDirectMessageAttribute:
                    await e.Context.RespondAsync("Psst. You need to be in DMs to run this!");
                    break;
                case RequireGuildAttribute:
                    await e.Context.RespondAsync("Not exactly sure what's that's supposed to accomplish in DMs; try it in a server.");
                    break;
            }
        }


        private async Task OnClientErrored(DiscordClient c, ClientErrorEventArgs e)
        {
            if (e.Exception.Message.Contains("event"))
            {
                _logger.LogWarning("[{Event}] Timed out!", e.EventName);
            }
            else
            {
                _logger.LogWarning(e.Exception, "Client threw an excpetion!");
            }
        }
        private async Task OnSocketErrored(DiscordClient c, SocketCloseEventArgs e)
        {
            if (e.CloseCode is 4014)
                _logger.LogCritical("Missing intents! Enable them on the developer dashboard (discord.com/developers/applications/{AppId})", _client.CurrentApplication.Id);
        }
        private async Task SendHelpAsync(DiscordClient c, string commandName, CommandContext originalContext)
        {
            CommandsNextExtension? cnext = c.GetCommandsNext();
            Command? cmd = cnext.RegisteredCommands["help"];
            CommandContext? ctx = cnext.CreateContext(originalContext.Message, null, cmd, commandName);
            await cnext.ExecuteCommandAsync(ctx);
        }
        public async Task SubscribeToEventsAsync()
        {
            TaskScheduler.UnobservedTaskException += async (_, e) => _logger.LogError("Task Scheduler caught an unobserved exception: {Exception}", e.Exception);

            IEnumerable<CommandsNextExtension?> commandsNext = (await _client.GetCommandsNextAsync()).Values;

            foreach (CommandsNextExtension? c in commandsNext)
                c!.CommandErrored += OnCommandErrored;
        }
    }
}