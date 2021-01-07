using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Humanizer;
using Humanizer.Localisation;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Silk.Core.Utilities
{
    public class BotExceptionHelper
    {
        private readonly ILogger<BotExceptionHelper> _logger;
        private readonly DiscordShardedClient _client;

        public BotExceptionHelper(ILogger<BotExceptionHelper> logger, DiscordShardedClient client) =>
            (_logger, _client) = (logger, client);

        public async Task OnCommandErrored(CommandsNextExtension c, CommandErrorEventArgs e)
        {
            if (e.Exception is CommandNotFoundException)
                _logger.LogWarning($"Command not found: Message: {e.Context.Message.Content}");

            if (e.Exception is InvalidOperationException && e.Exception.Message.Contains("command"))
            {
                _logger.LogWarning($"Command not found: Message {e.Context.Message.Content}");
                _ = SendHelpAsync(c.Client, e.Command.QualifiedName, e.Context);
            }

            if (e.Exception is ArgumentException) _ = SendHelpAsync(c.Client, e.Command.QualifiedName, e.Context);

            if (e.Exception is ChecksFailedException cf)
            {
                switch (cf.FailedChecks[0])
                {
                    case RequireOwnerAttribute:
                        DiscordUser owner = c.Client.CurrentApplication.Owners.First();
                        await e.Context.RespondAsync($"{e.Context.User.Username} doesn't look like {owner.Username}#{owner.Discriminator} to me!");
                        break;
                    case RequireNsfwAttribute:
                        await e.Context.RespondAsync("Hot, but this channel isn't that spicy! (Mark it as NSFW and I'll budge ;3)");
                        break;
                    case RequireFlagAttribute f:
                        await e.Context.RespondAsync($"Heh. You need to be {f.RequisiteUserFlag} for that.");
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
        }

        private async Task OnClientErrored(DiscordClient c, ClientErrorEventArgs e)
        {
            if (e.Exception.Message.Contains("event"))
                _logger.LogWarning($"[{e.EventName}] Timed out!");
            else if (e.Exception.Message.Contains("intents"))
                _logger.LogCritical("Missing intents! Enabled them on the developer dashboard.");
            else
                _logger.LogWarning($"{e.Exception.Message}");
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
            _client.ClientErrored += OnClientErrored;
            _client.Resumed += async (_, _) =>
                _logger.LogInformation("Reconnected."); // Async keyword because I'm lazy, and then I don't need to return anything.

            TaskScheduler.UnobservedTaskException += async (_, e) =>
                _logger.LogError("Task Scheduler caught an unobserved exception: " + e.Exception);

            IEnumerable<CommandsNextExtension?> commandsNext = (await _client.GetCommandsNextAsync()).Values;

            foreach (CommandsNextExtension? c in commandsNext)
                c!.CommandErrored += OnCommandErrored;
        }
    }
}