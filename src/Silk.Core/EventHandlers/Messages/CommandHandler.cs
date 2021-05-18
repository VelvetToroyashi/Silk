using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MediatR;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.MediatR.CommandInvocations;
using Silk.Core.Services.Interfaces;

namespace Silk.Core.EventHandlers.Messages
{
    public class CommandHandler
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CommandHandler> _logger;
        private readonly IPrefixCacheService _prefixService;
        public CommandHandler(IPrefixCacheService prefixService, IMediator mediator, ILogger<CommandHandler> logger)
        {
            _prefixService = prefixService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Handle(DiscordClient client, MessageCreateEventArgs args)
        {
            bool isBot = args.Author.IsBot;
            bool isEmpty = string.IsNullOrEmpty(args.Message.Content);
            DiscordUser bot = client.CurrentUser;
            if (isBot || isEmpty) return;

            var commandsNext = client.GetCommandsNext();
            string prefix = _prefixService.RetrievePrefix(args.Guild?.Id);

            int prefixLength =
                args.Channel.IsPrivate ? 0 :
                    args.MentionedUsers.Contains(bot) ?
                        args.Message.GetMentionPrefixLength(bot) :
                        args.Message.GetStringPrefixLength(prefix);

            if (prefixLength is -1) return;

            string commandString = args.Message.Content[prefixLength..];

            Command? command = commandsNext.FindCommand(commandString, out string arguments);

            if (command is null)
            {
                _logger.LogWarning("Could not find command. Message: {Message}", args.Message.Content);
                return;
            }

            CommandContext context = commandsNext.CreateContext(args.Message, prefix, command, arguments);

            await _mediator.Send(new AddCommandInvocationRequest(args.Author.Id, args.Guild?.Id, command!.QualifiedName), CancellationToken.None);

            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMinutes(10));

            _ = Task.Run(async () => await commandsNext.ExecuteCommandAsync(context), CancellationToken.None);
        }
    }
}