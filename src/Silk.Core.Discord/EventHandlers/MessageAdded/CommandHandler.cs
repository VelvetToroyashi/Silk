using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using MediatR;
using Serilog;
using Silk.Core.Data.MediatR.CommandInvocations;
using Silk.Core.Data.Models;
using Silk.Core.Discord.EventHandlers.Notifications;
using Silk.Core.Discord.Services;
using Silk.Core.Discord.Services.Interfaces;

namespace Silk.Core.Discord.EventHandlers.MessageAdded
{
    public class CommandHandler : INotificationHandler<MessageCreated>
    {
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
            bool isBot = notification.Event.Author.IsBot;
            bool isEmpty = string.IsNullOrEmpty(notification.Event.Message.Content);
            DiscordUser bot = notification.Client.CurrentUser;
            if (isBot || isEmpty) return;

            var commandsNext = notification.Client.GetCommandsNext();
            string prefix = _prefixService.RetrievePrefix(notification.Event.Guild.Id);

            int prefixLength =
                notification.Event.Channel.IsPrivate ? 0 :
                    notification.Event.MentionedUsers.Contains(bot) ?
                        notification.Event.Message.GetMentionPrefixLength(bot) :
                        notification.Event.Message.GetStringPrefixLength(prefix);

            if (prefixLength is -1) return;

            string commandString = notification.Event.Message.Content[prefixLength..];

            if (notification.Event.Guild is not null)
            {
                GuildConfig config = await _cache.GetConfigAsync(notification.Event.Guild.Id);
                if (config.DisabledCommands.Any(c => commandString.Contains(c.CommandName)))
                {
                    return;
                }
            }

            Command? command = commandsNext.FindCommand(commandString, out string arguments);

            if (command is null)
            {
                Log.Logger.ForContext(typeof(CommandHandler)).Warning("Could not find command. Message: {@Message}", notification);
                return;
            }

            var context = commandsNext.CreateContext(notification.Event.Message, prefix, command, arguments);

            await _mediator.Send(new AddCommandInvocationRequest(notification.Event.Author.Id, notification.Event.Guild?.Id, command!.QualifiedName), CancellationToken.None);

            _ = Task.Run(async () => await commandsNext.ExecuteCommandAsync(context), CancellationToken.None);
        }
    }
}