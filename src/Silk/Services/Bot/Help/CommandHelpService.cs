using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Remora.Commands.Conditions;
using Remora.Commands.Results;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Infrastructure;

namespace Silk.Services.Bot.Help;

public class CommandHelpService : ICommandHelpService
{
    private readonly TreeWalker _treeWalker;
    private readonly IServiceProvider _services;
    private readonly HelpSystemOptions _options;
    private readonly IDiscordRestChannelAPI _channels;
    
    
    public CommandHelpService
    (
        TreeWalker treeWalker,
        IServiceProvider services,
        IOptions<HelpSystemOptions> options,
        IDiscordRestChannelAPI channels
    )
    {
        _treeWalker = treeWalker;
        _services   = services;
        _options    = options.Value;
        _channels   = channels;
    }

    public async Task<Result> ShowHelpAsync(Snowflake channelID, string? commandName = null, string? treeName = null)
    {
        var nodes = _treeWalker.FindNodes(commandName, treeName);

        if (!_options.AlwaysShowCommands)
        {
            var evaluated = await EvaluateNodeConditionsAsync(nodes);
            
            var evaluatedNodes = evaluated.Nodes.ToArray();
            
            if (!string.IsNullOrEmpty(commandName) && nodes.Count > evaluatedNodes.Length)
                return Result.FromError(new ConditionNotSatisfiedError("One or more conditions were not satisfied.", evaluated.FirstFailedCondition!));

            nodes = evaluatedNodes;
        }
        
        if (!nodes.Any())
            return Result.FromError(new NotFoundError($"No command with the name \"{commandName}\" was found."));

        var formatter = _services.GetService<IHelpFormatter>();
        
        if (formatter is null)
            return Result.FromError(new InvalidOperationError("Help was invoked, but no formatter was registered."));

        IEnumerable<IEmbed> embeds;

        embeds = (nodes.Count, string.IsNullOrEmpty(commandName), nodes.FirstOrDefault() is IParentNode) switch
        {
            (> 1, true,  _) => formatter.GetTopLevelHelpEmbeds(nodes.GroupBy(n => n.Key)),
            (> 1, false, _) => formatter.GetCommandHelp(nodes),
            (  1, _, false) => new [] {formatter.GetCommandHelp(nodes.Single()) },
            (  1, _,  true) => formatter.GetCommandHelp(nodes)
        };

        var sendResult = await _channels.CreateMessageAsync(channelID, embeds: embeds.ToArray());

        return sendResult.IsSuccess ? Result.FromSuccess() : Result.FromError(sendResult.Error);
    }
    
    public async Task<(IEnumerable<IChildNode> Nodes, ConditionAttribute? FirstFailedCondition)> EvaluateNodeConditionsAsync(IReadOnlyList<IChildNode> nodes)
    {
        var returnNode      = default(ConditionAttribute?);
        
        var successfulNodes = new HashSet<IChildNode>();

        foreach (var node in nodes)
        {
            var conditions = new List<ConditionAttribute>();

            if (node is CommandNode cn)
            {
                conditions.AddRange(cn.GroupType.GetCustomAttributes<ConditionAttribute>());
                conditions.AddRange(cn.CommandMethod.GetCustomAttributes<ConditionAttribute>());
            }
            
            if (node is GroupNode gn)
                conditions.AddRange(gn.GroupTypes.SelectMany(gt => gt.GetCustomAttributes<ConditionAttribute>()));

            if (!conditions.Any())
            {
                successfulNodes.Add(node);
                continue;
            }

            foreach (var setCondition in conditions)
            {
                var conditionType = typeof(ICondition<>).MakeGenericType(setCondition.GetType());
                var conditionMethod = conditionType.GetMethod(nameof(ICondition<ConditionAttribute>.CheckAsync));
                
                var conditionServices = _services
                                        .GetServices(conditionType)
                                        .Where(c => c is not null)
                                        .Cast<ICondition>()
                                        .ToArray();
                
                if (!conditionServices.Any())
                    throw new InvalidOperationException($"Command was marked with \"{conditionType.Name}\", but no service was registered to handle it.");

                foreach (var condition in conditionServices)
                {
                    var result = await (ValueTask<Result>) conditionMethod!.Invoke(condition, new object[] {setCondition, CancellationToken.None})!;

                    if (result.IsSuccess)
                    {
                        successfulNodes.Add(node);
                    }
                    else
                    {
                        returnNode = setCondition;
                        successfulNodes.Remove(node);
                        
                        goto next;
                    }
                }

                continue;
                
                next:
                break;
            }
        }

        return (successfulNodes, returnNode);
    }
}