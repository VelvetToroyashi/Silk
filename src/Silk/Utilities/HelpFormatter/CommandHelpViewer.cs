using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Conditions;
using Remora.Commands.Trees;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using Remora.Results;

namespace Silk.Utilities.HelpFormatter;

public class CommandHelpViewer
{
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly IServiceProvider       _services;
    private readonly CommandTree            _tree;

    public CommandHelpViewer(CommandTree tree, IServiceProvider services, IDiscordRestChannelAPI channelApi)
    {
        _tree       = tree;
        _services   = services;
        _channelApi = channelApi;
    }

    public async Task<Result<IMessage>> SendHelpAsync(string? command, Snowflake channelID)
    {
        var formatter = _services.GetService<IHelpFormatter>();

        if (formatter is null)
            return Result<IMessage>.FromError(new InvalidOperationError("No help formatter was registered with the container."));

        IEmbed embed;

        if (command is null)
        {
            IEnumerable<IChildNode> commands = await GetApplicableCommandsAsync(_services, _tree.Root.Children);

            if (!commands.Any())
                return Result<IMessage>.FromError(new InvalidOperationError("No commands were found."));

            embed = formatter.GetHelpEmbed(commands);
        }
        else
        {
            IChildNode? node = GetCommands(command, _tree.Root);

            if (node is null) //TODO: Change this message to "No command was found with the name '<name>'."
                return Result<IMessage>.FromError(new NotFoundError("The specified command does not exist."));

            IEnumerable<IChildNode> childCommands = await GetApplicableCommandsAsync(_services, node is IParentNode parent ? parent.Children : new[] { node });

            if (!childCommands.Any())
                return Result<IMessage>.FromError(new InvalidOperationError("No commands were found."));

            embed = node is IParentNode
                ? formatter.GetHelpEmbed(childCommands)
                : formatter.GetHelpEmbed((CommandNode)node);
        }

        Result<IMessage> res = await _channelApi.CreateMessageAsync(channelID, embeds: new[] { embed });

        return res;
    }

    public IChildNode? GetCommands(string command, IParentNode parent)
    {
        if (!command.Contains(' ')) // Top level command, only search the immediate children
            return parent.Children.FirstOrDefault(x => command.Equals(x.Key, StringComparison.OrdinalIgnoreCase) ||
                                                       x.Aliases.Contains(command, StringComparer.OrdinalIgnoreCase));
        string[]? commandRoute = command.Split(' ');

        foreach (string token in commandRoute)
            foreach (IChildNode child in parent.Children)
                if (token.Equals(child.Key) || child.Aliases.Contains(token, StringComparer.OrdinalIgnoreCase))
                    if (child is IParentNode pn)
                        return GetCommands(string.Join(" ", commandRoute.Skip(1)), pn) ?? child;
                    else return child;

        return null;
    }

    public async Task<IEnumerable<IChildNode>> GetApplicableCommandsAsync
        (
            IServiceProvider        services,
            IEnumerable<IChildNode> nodes
        )
    {
        var commands = new List<IChildNode>();

        foreach (IChildNode node in nodes)
        {
            var conditionsToEvaluate = new List<ConditionAttribute>();
            if (node is GroupNode gn)
            {
                IEnumerable<ConditionAttribute> groupAttributes = gn.GetType().GetCustomAttributes<ConditionAttribute>(false);

                if (!groupAttributes.Any())
                {
                    commands.Add(gn);
                    continue;
                }

                conditionsToEvaluate.AddRange(groupAttributes);
            }
            else
            {
                var command = node as CommandNode;

                IEnumerable<ConditionAttribute> commandAttributes = command.CommandMethod.GetCustomAttributes<ConditionAttribute>();

                if (!commandAttributes.Any())
                {
                    commands.Add(command);
                    continue;
                }

                conditionsToEvaluate.AddRange(commandAttributes);
            }

            foreach (ConditionAttribute conditionToEvaluate in conditionsToEvaluate)
            {
                Type conditionType = typeof(ICondition<>).MakeGenericType(conditionToEvaluate.GetType());

                ICondition[] conditionServices = services
                                                .GetServices(conditionType)
                                                .Where(c => c is not null)
                                                .Cast<ICondition>()
                                                .ToArray();

                if (!conditionServices.Any())
                    throw new InvalidOperationException($"Command was marked with {conditionToEvaluate.GetType().Name}, but no service was registered to handle it.");

                var add = true;
                foreach (ICondition? condition in conditionServices)
                {
                    MethodInfo? method = typeof(ICondition<>).MakeGenericType(conditionToEvaluate.GetType()).GetMethod(nameof(ICondition<ConditionAttribute>.CheckAsync));
                    Result      res    = await (ValueTask<Result>)method!.Invoke(condition, new object[] { conditionToEvaluate, CancellationToken.None })!;

                    if (!res.IsSuccess)
                    {
                        add = false;
                        break;
                    }
                }

                if (add) commands.Add(node);
            }
        }

        return commands;
    }
}