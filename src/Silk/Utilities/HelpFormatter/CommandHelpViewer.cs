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

        IEnumerable<IEmbed> embeds;

        if (command is null)
        {
            IEnumerable<IChildNode> commands = await GetApplicableCommandsAsync(_services, _tree.Root.Children);

            if (!commands.Any())
                return Result<IMessage>.FromError(new InvalidOperationError("No commands were found."));

            commands = commands.DistinctBy(c => c.Key, StringComparer.OrdinalIgnoreCase);
            
            embeds = new [] { formatter.GetSubcommandHelpEmbed(commands) };
        }
        else
        {
            var nodes = SearchCommands(command, _tree.Root);

            if (!nodes.Any()) //TODO: Change this message to "No command was found with the name '<name>'."
                return Result<IMessage>.FromError(new NotFoundError($"No command was found with the name '{command}'."));

            IEnumerable<IChildNode> viewableCommands = await GetApplicableCommandsAsync(_services, nodes);

            if (!viewableCommands.Any())
                return Result<IMessage>.FromError(new InvalidOperationError("No commands were found."));

            embeds = nodes.First() is IParentNode
                ? new [] { formatter.GetSubcommandHelpEmbed(viewableCommands.Skip(1)) }
                : formatter.GetCommandHelpEmbeds(viewableCommands.Count() > 1 
                                                     ? viewableCommands.Cast<CommandNode>().ToArray() 
                                                     : viewableCommands.Cast<CommandNode>().First());
        }

        Result<IMessage> res = await _channelApi.CreateMessageAsync(channelID, embeds: embeds.ToArray());

        return res;
    }

    public IEnumerable<IChildNode> SearchCommands(string command, IParentNode parent)
    {
        string[] commandRoute = command.Split(' ');

        var currentToken = commandRoute[0];

        foreach (var child in parent.Children)
        {
            if (string.Equals(child.Key, currentToken, StringComparison.OrdinalIgnoreCase) ||
                child.Aliases.Contains(currentToken, StringComparer.OrdinalIgnoreCase))
            {
                if (child is not IParentNode pn)
                {
                    yield return child;
                }
                else
                {
                    if (commandRoute.Length is 1)
                    {
                        yield return child;
                        
                        foreach (var subcommand in pn.Children)
                            yield return subcommand;

                        yield break;
                    }

                    var commands = SearchCommands(string.Join(' ', commandRoute.Skip(1)), pn);
                    foreach (var subCommand in commands)
                        yield return subCommand;
                }
            }
        }
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