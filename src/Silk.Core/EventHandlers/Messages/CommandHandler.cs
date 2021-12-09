using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MediatR;
using Microsoft.Extensions.Logging;
using Silk.Core.Services.Interfaces;
using Silk.Shared.Constants;

namespace Silk.Core.EventHandlers.Messages;

public sealed class CommandHandler
{
    private readonly ILogger<CommandHandler> _logger;
    private readonly IMediator               _mediator;
    private readonly IPrefixCacheService     _prefixService;

    public CommandHandler(DiscordClient client, IPrefixCacheService prefixService, IMediator mediator, ILogger<CommandHandler> logger)
    {
        client.MessageCreated += Handle;
        _prefixService = prefixService;
        _mediator = mediator;
        _logger = logger;
    }
        
    public async Task Handle(DiscordClient client, MessageCreateEventArgs args)
    {
        bool isBot = args.Author.IsBot;
        bool isEmpty = string.IsNullOrEmpty(args.Message.Content);

        if (isBot || isEmpty)
            return;

        DiscordMember? bot = args.Guild?.CurrentMember;
        CommandsNextExtension? commandsNext = client.GetCommandsNext();
        string prefix = _prefixService.RetrievePrefix(args.Guild?.Id);

        int prefixLength = GetPrefixLength(prefix, args.Message, bot);

        if (prefixLength is -1)
            return;

        string commandString = args.Message.Content[prefixLength..];

        Command? command = commandsNext.FindCommand(commandString, out string arguments);

        if (command is null)
        {
            _logger.LogWarning(EventIds.Service, "Could not find command. Message: {Message}", args.Message.Content);
            return;
        }

        CommandContext context = commandsNext.CreateContext(args.Message, prefix, command, arguments);

        _ = Task.Run(async () => await commandsNext.ExecuteCommandAsync(context));
    }

    private static int GetPrefixLength(string prefix, DiscordMessage message, DiscordMember? currentUser)
    {
        if (message.Channel is DiscordDmChannel)
            return 0;

        return message.Content.StartsWith(currentUser?.Mention!) ?
            message.GetMentionPrefixLength(currentUser) :
            message.GetStringPrefixLength(prefix);
    }
}