using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Conditions;
using Remora.Commands.Services;
using Remora.Commands.Trees;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
using Remora.Results;

namespace Silk.Core.Utilities.HelpFormatter
{
    public class CommandHelpViewer
    {
        private static readonly Func<ICondition, ConditionAttribute, CancellationToken, ValueTask<Result>> _evaluateConditionAsync = 
            (Func<ICondition, ConditionAttribute, CancellationToken, ValueTask<Result>>)
            typeof(ICondition<>)
               .GetMethod(nameof(ICondition<ConditionAttribute>.CheckAsync))!
               .CreateDelegate(typeof(Func<ICondition, ConditionAttribute, CancellationToken, ValueTask<Result>>));
        
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly CommandService         _commands;
        private readonly IServiceProvider       _services;
        private readonly CommandTree            _tree;
        
        public CommandHelpViewer(CommandTree tree, CommandService commands, IServiceProvider services, IDiscordRestChannelAPI channelApi)
        {
            _tree = tree;
            _commands = commands;
            _services = services;
            _channelApi = channelApi;
        }

        public async Task<Result<IMessage>> SendHelpAsync(string? command, Snowflake channelID)
        {
            var formatter = _services.GetService<IHelpFormatter>();

            if (formatter is null)
                return Result<IMessage>.FromError(new InvalidOperationError("No help formatter was registered with the container."));

            IEmbed embed = null;

            if (command is null)
            {
                
            }
            else
            {
                IChildNode? node = GetCommands(command, _tree.Root);

                if (node is null) //TODO: Change this message to "No command was found with the name '<name>'."
                    return Result<IMessage>.FromError(new NotFoundError("The specified command does not exist."));

                embed = node is GroupNode gn ? formatter.GetHelpEmbed(gn.Children) : formatter.GetHelpEmbed((CommandNode)node);
            }

            return await _channelApi.CreateMessageAsync(channelID, embeds: new[] { embed });
        }

        public IChildNode? GetCommands(string command, IParentNode parent)
        {
            if (!command.Contains(' ')) // Top level command, only search the immediate children
                return parent.Children.FirstOrDefault(x => command.Equals(x.Key, StringComparison.OrdinalIgnoreCase) ||
                                                           x.Aliases.Contains(command, StringComparer.OrdinalIgnoreCase));
            string[]? commandRoute = command.Split(' ');

            foreach (var token in commandRoute)
                foreach (var child in parent.Children)
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

            foreach (var node in nodes)
            {
                var conditionsToEvaluate = new List<ConditionAttribute>();
                if (node is GroupNode gn)
                {
                    var groupAttributes = gn.GetType().GetCustomAttributes<ConditionAttribute>(false);

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

                    var commandAttributes = command.CommandMethod.GetCustomAttributes<ConditionAttribute>();
                    
                    if (!commandAttributes.Any())
                    {
                        commands.Add(command);
                        continue;
                    }
                    
                    conditionsToEvaluate.AddRange(commandAttributes);
                }

                foreach (var contionToEvaluate in conditionsToEvaluate)
                {
                    var conditionType = typeof(ICondition<>).MakeGenericType(contionToEvaluate.GetType());
                    
                    var conditionServices = services
                                           .GetServices(conditionType)
                                           .Where(c => c is not null)
                                           .Cast<ICondition>()
                                           .ToArray();

                    if (!conditionServices.Any())
                        throw new InvalidOperationException($"Command was marked with {contionToEvaluate.GetType().Name}, but no service was registered to handle it.");

                    bool add = true;
                    foreach (var condition in conditionServices)
                    {
                        var res = await _evaluateConditionAsync(condition, contionToEvaluate, CancellationToken.None);

                        if (res.IsSuccess)
                        {
                            add = false;
                            break;
                        }
                    }
                    
                    if (add)
                        commands.Add(node);
                }
            }

            return commands;
        }
    }
}