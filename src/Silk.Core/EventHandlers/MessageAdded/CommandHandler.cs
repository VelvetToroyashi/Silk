using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using MediatR;
using Silk.Core.EventHandlers.Notifications;
using Silk.Core.Services;
using Silk.Core.Services.Interfaces;
using Silk.Data.MediatR;
using Silk.Data.Models;

namespace Silk.Core.EventHandlers.MessageAdded
{
        public class CommandHandler : INotificationHandler<MessageCreated>
        {
            private readonly IPrefixCacheService _prefixService;
            private readonly ConfigService _cache;
            private readonly IMediator _mediator;
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

                string prefix = _prefixService.RetrievePrefix(notification.EventArgs.Guild?.Id ?? 0);

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
                
                CommandContext context = command is null ?
                    throw new CommandNotFoundException($"Invalid command {commandString}") :
                    commandsNext.CreateContext(notification.EventArgs.Message, prefix, command, arguments);
                
                _ = Task.Run(async () =>
                    {
                        await _mediator.Send(new CommandInvokeRequest.Add(notification.EventArgs.Author.Id, notification.EventArgs.Guild?.Id, command.QualifiedName), CancellationToken.None);
                        await commandsNext.ExecuteCommandAsync(context);
                    }, CancellationToken.None)
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted && t.Exception?.InnerException is not null) throw t.Exception.InnerException;
                    }, cancellationToken);
            }
        }
}