using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using MediatR;
using Silk.Core.Data.MediatR.Unified.CommandInvocations;
using Silk.Core.Data.Models;
using Silk.Core.Discord.EventHandlers.Notifications;
using Silk.Core.Discord.Services;
using Silk.Core.Discord.Services.Interfaces;

namespace Silk.Core.Discord.EventHandlers.MessageAdded
{
    public class CommandHandler : INotificationHandler<MessageCreated>
    {
        //Message Content, Exception

        //Also in retrospect, this could've been a static method on BotExceptionHandler, but I digress.
        public static Action<string, Exception> ParserErrored = (_, _) => { };
        private readonly ConfigService _cache;
        private readonly IMediator _mediator;

        private readonly IPrefixCacheService _prefixService;
        public CommandHandler(IPrefixCacheService prefixService, IMediator mediator, ConfigService cache)
        {
            _prefixService = prefixService;
            _mediator = mediator;
            _cache = cache;
        }

        public async Task Handle(MessageCreated notification, CancellationToken cancellationToken)
        {

            bool isBot = notification.EventArgs.Author.IsBot;
            bool isEmpty = string.IsNullOrEmpty(notification.EventArgs.Message.Content);
            DiscordUser bot = notification.Client.CurrentUser;
            if (isBot || isEmpty) return;

            var commandsNext = notification.Client.GetCommandsNext();

            string prefix = _prefixService.RetrievePrefix(notification.EventArgs.Guild?.Id);

            int prefixLength =
                notification.EventArgs.Channel.IsPrivate ? 0 :
                    notification.EventArgs.MentionedUsers.Contains(bot) ?
                        notification.EventArgs.Message.GetMentionPrefixLength(bot) :
                        notification.EventArgs.Message.GetStringPrefixLength(prefix);

            if (prefixLength is -1) return;

            string commandString = notification.EventArgs.Message.Content[prefixLength..];

            if (notification.EventArgs.Guild is not null)
            {
                GuildConfig config = await _cache.GetConfigAsync(notification.EventArgs.Guild.Id);
                if (config.DisabledCommands.Any(c => commandString.Contains(c.CommandName)))
                {
                    return;
                }
            }

            Command? command = commandsNext.FindCommand(commandString, out string arguments);

            object context = command is null ?
                new CommandNotFoundException($"Invalid command {commandString}") :
                commandsNext.CreateContext(notification.EventArgs.Message, prefix, command, arguments);

            if (context is CommandNotFoundException cnf)
            {
                ParserErrored(notification.EventArgs.Message.Content, cnf);
                return;
            }

            await _mediator.Send(new AddCommandInvocationRequest(notification.EventArgs.Author.Id, notification.EventArgs.Guild?.Id, command!.QualifiedName), CancellationToken.None);


            _ = Task.Run(async () =>
            {
                await commandsNext.ExecuteCommandAsync(context as CommandContext);
            }, CancellationToken.None);
        }
    }
}