using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using MediatR;
using Serilog;
using Silk.Core.Data.MediatR.CommandInvocations;
using Silk.Core.Data.Models;
using Silk.Core.Discord.EventHandlers.Notifications;
using Silk.Core.Discord.Services;
using Silk.Core.Discord.Services.Interfaces;
using Silk.Shared.Abstractions.DSharpPlus.Concrete;
using User = Silk.Shared.Abstractions.DSharpPlus.Concrete.User;

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

            bool isBot = notification.Message.Author.IsBot;
            bool isEmpty = string.IsNullOrEmpty(notification.Message.Content);
            User bot = notification.Client.CurrentUser;
            if (isBot || isEmpty) return;

            var commandsNext = notification.Client.GetCommandsNext();

            string prefix = _prefixService.RetrievePrefix(notification.Message.GuildId);


            int prefixLength =
                notification.Message.Channel.IsPrivate ? 0 :
                    notification.Message.MentionedUsers.Contains(bot) ?
                        GetStringMentionLength(notification.Message, bot) :
                        GetStringPrefixLength(notification.Message, prefix);

            if (prefixLength is -1) return;

            string commandString = notification.Message.Content[prefixLength..];

            if (notification.Message.Guild is not null)
            {
                GuildConfig config = await _cache.GetConfigAsync(notification.Message.Guild.Id);
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

            var context = commandsNext.CreateContext(notification.Message, prefix, command, arguments);

            await _mediator.Send(new AddCommandInvocationRequest(notification.Message.Author.Id, notification.Message.Guild?.Id, command!.QualifiedName), CancellationToken.None);

            _ = Task.Run(async () => await commandsNext.ExecuteCommandAsync(context), CancellationToken.None);
        }
        private int GetStringPrefixLength(Message message, string prefix) => message.Content.StartsWith(prefix) ? prefix.Length : -1;
        private int GetStringMentionLength(Message message, User user) => message.Content.StartsWith(user.Mention) ? user.Mention.Length : -1;
    }

}