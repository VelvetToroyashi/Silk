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

        var embeds = Array.Empty<IEmbed>();

        if (string.IsNullOrEmpty(command))
        {
            IEnumerable<IChildNode> commands = await GetApplicableCommandsAsync(_services, _tree.Root.Children);

            if (!commands.Any())
                return Result<IMessage>.FromError(new InvalidOperationError("No commands were found."));

            commands = commands.DistinctBy(c => c.Key, StringComparer.OrdinalIgnoreCase);
            
            embeds = new [] { formatter.GetSubcommandHelpEmbed(commands) };
        }
        else
        {
            var nodes = FindNodes(command, _tree.Root, out var isSubcommands);

            if (!nodes.Any())
                return Result<IMessage>.FromError(new NotFoundError($"No command was found with the name '{command}'."));

            if (isSubcommands)
                embeds = new[] { formatter.GetSubcommandHelpEmbed(nodes.DistinctBy(node => node.Key)) };
            else
                embeds = formatter.GetCommandHelpEmbeds(nodes.Count is 1 
                                                            ? (CommandNode)nodes.Single() 
                                                            : nodes.Cast<CommandNode>().ToArray()).ToArray();
        }

        Result<IMessage> res = await _channelApi.CreateMessageAsync(channelID, embeds: embeds.ToArray());

        return res;
    }
        
    public IReadOnlyList<IChildNode> FindNodes(string command, IParentNode parent, out bool isSubcommands)
    {
        isSubcommands = false;
        
        var nodes      = new List<IChildNode>();
        var tokenStack = new Stack<string>(command.Split(' ').Reverse());

        while (tokenStack.TryPop(out var current))
        {
            foreach (var child in parent.Children)
            {
                if (child.Key.Equals(current, StringComparison.OrdinalIgnoreCase) ||
                    child.Aliases.Contains(current, StringComparer.OrdinalIgnoreCase))
                {
                    if (tokenStack.TryPeek(out _))
                    {
                        if (child is not IParentNode pn)
                            continue;

                        parent = pn;
                        break;
                    }
                    
                    isSubcommands |= child is IParentNode;

                    if (child is not IParentNode group || nodes.Any(c => c.Key.Equals(child.Key, StringComparison.OrdinalIgnoreCase)))
                        nodes.Add(child);
                    else
                        nodes.AddRange(group.Children);
                }
            } 
        }
        
        return nodes;
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
            
            // Bug: Groups' parent's conditions are not evaluated. 
            // This method does not reverse-walk the tree to find the parent's conditions.
            // If your name is Jax, or you happen to know the best way to fix this, please PR it.
            // If you're having an issue with groups being exposed, but not executable, it's likely
            // that you need to add a condition on the nested group. Adding it on the parent has no 
            // effect on it's children.
            if (node is GroupNode gn)
            {
                IEnumerable<ConditionAttribute> groupAttributes = gn.GroupTypes.SelectMany(gt => gt.GetCustomAttributes<ConditionAttribute>(true));

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

            var add = true;
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
            }
            
            if (add)
                commands.Add(node);
        }

        return commands;
    }
}