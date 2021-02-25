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
    //TODO: Fix this, I guess. Clean it up****
    public class BotExceptionHandler
    {
        private static ILogger<BotExceptionHandler> _logger = null!;
        private readonly DiscordShardedClient _client;

        public BotExceptionHandler(ILogger<BotExceptionHandler> logger, DiscordShardedClient client) => (_logger, _client) = (logger, client);

        public async Task OnCommandErrored(CommandsNextExtension c, CommandErrorEventArgs e)
        {
            if (e.Exception is CommandNotFoundException cnfe)
            {
                _logger.LogWarning($"Command Not Found: Message: {e.Context.Message.Content}\nException: {cnfe.Message}");
            }
            else if (e.Exception is InvalidOperationException iope && iope.Message.Contains("command"))
            {
                _logger.LogWarning($"Invalid Command Operation: Message {e.Context.Message.Content}\nException: {iope.Message}");
                _ = SendHelpAsync(c.Client, e.Command.QualifiedName, e.Context);
            }
            else if (e.Exception.Message is "Could not find a suitable overload for the command.")
            {
                _logger.LogWarning($"Invalid Command Parameters {e.Command.Name} | {e.Context.RawArgumentString}");
                _ = SendHelpAsync(c.Client, e.Command.QualifiedName, e.Context);
            }
            else if (e.Exception is ArgumentException ae)
            {
                _logger.LogWarning(ae.Message);
            }
            else if (e.Exception is ChecksFailedException cf)
            {
                switch (cf.FailedChecks[0])
                {
                    case RequireOwnerAttribute:
                        string owner = c.Client.CurrentApplication.Owners.Select(o => $"{o.Username}#{o.Discriminator}").Join(", ");
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
            else Log.Logger.ForContext(e.Command.GetType()).Warning(e.Exception, "Soemthing went wrong:");
        }

        private async Task OnClientErrored(DiscordClient c, ClientErrorEventArgs e)
        {
            if (e.Exception.Message.Contains("event")) _logger.LogWarning($"[{e.EventName}] Timed out!");
            else if (e.Exception.Message.Contains("intents")) _logger.LogCritical("Missing intents! Enabled them on the developer dashboard");
            else _logger.LogWarning($"{e.Exception.Message}");
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
            _client.ClientErrored += OnClientErrored;

            TaskScheduler.UnobservedTaskException += async (_, e) => _logger.LogError("Task Scheduler caught an unobserved exception: " + e.Exception);
            IEnumerable<CommandsNextExtension?> commandsNext = (await _client.GetCommandsNextAsync()).Values;

            foreach (CommandsNextExtension? c in commandsNext)
                c!.CommandErrored += OnCommandErrored;
        }
    }
}