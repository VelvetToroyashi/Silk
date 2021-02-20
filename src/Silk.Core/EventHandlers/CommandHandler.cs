using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using MediatR;
using Silk.Core.EventHandlers.Notifications;
using Silk.Core.Services.Interfaces;

namespace Silk.Core.EventHandlers
{
        public class CommandHandler : INotificationHandler<MessageCreated>
        {
            private readonly IPrefixCacheService _prefixService;
            public CommandHandler(IPrefixCacheService prefixService)
            {
                _prefixService = prefixService;
            }

            public async Task Handle(MessageCreated notification, CancellationToken cancellationToken)
            {
                bool isBot = notification.EventArgs.Author.IsBot;
                bool isEmpty = string.IsNullOrEmpty(notification.EventArgs.Message.Content);
                DiscordUser bot = notification.Client.CurrentUser;
                if (isBot || isEmpty) return;

                var commandsNext = notification.Client.GetCommandsNext();

                string prefix = _prefixService.RetrievePrefix(notification.EventArgs.Guild?.Id ?? 0);

                int prefixLength =
                    notification.EventArgs.Channel.IsPrivate ? 0 :
                    notification.EventArgs.MentionedUsers.Contains(bot) ?
                    notification.EventArgs.Message.GetMentionPrefixLength(bot) : 
                    notification.EventArgs.Message.GetStringPrefixLength(prefix);

                if (prefixLength is -1) return;

                string commandString = notification.EventArgs.Message.Content[prefixLength..];

                Command? command = commandsNext.FindCommand(commandString, out string arguments);
                CommandContext context = command is null ?
                    throw new CommandNotFoundException($"Invalid command {commandString}") :
                    commandsNext.CreateContext(notification.EventArgs.Message, prefix, command, arguments);
                
                _ = Task.Run(async () => await commandsNext.ExecuteCommandAsync(context), CancellationToken.None)
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted && t.Exception?.InnerException is not null) throw t.Exception.InnerException;
                    }, cancellationToken);
            }
        }
}