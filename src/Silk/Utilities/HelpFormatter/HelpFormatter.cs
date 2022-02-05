using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using OneOf;
using Remora.Commands.Attributes;
using Remora.Commands.Signatures;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Results;
using Silk.Extensions;

namespace Silk.Utilities.HelpFormatter;

public class HelpFormatter : IHelpFormatter
{

    public IEnumerable<IEmbed> GetCommandsHelpEmbeds(OneOf<CommandNode, IReadOnlyList<IChildNode>> commands)
    {
        if (commands.TryPickT0(out var standalone, out var overloads))
        {
            string aliases = standalone.Aliases.Any() ? string.Join(", ", standalone.Aliases) + '\n' : "None";

            var embed = new Embed
            {
                //Title = command.Key,
                Description = $"**Command** - {standalone.Key}\n"               +
                              $"**Aliases** - {aliases}\n"                      +
                              $"**Usage**\n {GetParameterHelp(standalone)}\n\n" +
                              $"**Description** - {standalone.Shape.Description}", //command.Shape.Description + usage + parameterHelp,
                Colour = Color.DodgerBlue
            };

            yield return embed;
        }
        else
        {
            if (overloads.All(c => c is CommandNode)) // If this is the case, we're dealing with actual overloads.
            {
                var    overloadIndex = 0;
                var    firstOverload = overloads.First();
                string aliases       = firstOverload.Aliases.Any() ? string.Join(", ", firstOverload.Aliases) + '\n' : "None";
            
                yield return new Embed
                {
                    Title = "This command has several ways to use it:",
                    Description = $"**Command** - {firstOverload.Key}\n" +
                                  $"**Aliases** - {aliases}\n",                         
                    Colour = Color.DodgerBlue
                };

                foreach (var overload in overloads.Cast<CommandNode>())
                {
                    yield return new Embed
                    {
                        Title = $"Option {++overloadIndex}/{overloads.Count}",
                        Description = $"**Usage**\n {GetParameterHelp(overload)}\n\n" +
                                      $"**Description** - {overload.Shape.Description}",
                        Colour = Color.DodgerBlue
                    };
                }
            }


        }
    }

    /// <inheritdoc />
    public IEnumerable<IEmbed> GetCommandHelpEmbeds(OneOf<CommandNode, IReadOnlyList<CommandNode>> command)
    {
        if (command.TryPickT0(out var standalone, out var overloads))
        {
            string aliases = standalone.Aliases.Any() ? string.Join(", ", standalone.Aliases) + '\n' : "None";

            var embed = new Embed
            {
                //Title = command.Key,
                Description = $"**Command** - {standalone.Key}\n"               +
                              $"**Aliases** - {aliases}\n"                      +
                              $"**Usage**\n {GetParameterHelp(standalone)}\n\n" +
                              $"**Description** - {standalone.Shape.Description}", //command.Shape.Description + usage + parameterHelp,
                Colour = Color.DodgerBlue
            };

            yield return embed;
        }
        else
        {
            var    overloadIndex = 0;
            var    firstOverload = overloads.First();
            string aliases       = firstOverload.Aliases.Any() ? string.Join(", ", firstOverload.Aliases) + '\n' : "None";
            
            yield return new Embed
            {
                Title = "This command has several ways to use it:",
                Description = $"**Command** - {firstOverload.Key}\n" +
                              $"**Aliases** - {aliases}\n",                         
                              Colour = Color.DodgerBlue
            };

            foreach (var overload in overloads)
            {
                yield return new Embed
                {
                    Title = $"Option {++overloadIndex}/{overloads.Count}",
                    Description = $"**Usage**\n {GetParameterHelp(overload)}\n\n" +
                                  $"**Description** - {overload.Shape.Description}",
                    Colour = Color.DodgerBlue
                };
            }
        }
    }
    
    /// <summary>
    /// Formats a given command's parameters into a man-pages style string.
    /// </summary>
    /// <param name="command">The command to format.</param>
    /// <returns>A string styled akin to man-pages, using &lt;&gt; and [] to denote optional and required parameters.</returns>
    private string GetParameterHelp(CommandNode command)
        => !command.Shape.Parameters.Any() 
            ? "This command can be used without parameters!"
            : string.Join("\n\n", command
                            .Shape
                            .Parameters
                            .Select(p => $"`{GetHumanFriendlyParemeterName(p)}` - {p.Description}"));
    
    
    /// <inheritdoc />
    public IEmbed GetSubcommandHelpEmbed(IEnumerable<IChildNode> subcommands)
    {
        IEmbed  embed;
        string? commandString = string.Join('\n', subcommands.Select(c => '`' + c.Key + '`'));

        if (subcommands.First().Parent is RootNode)
        {
            // Root node, display all top level commands

            var fields = new List<IEmbedField>();

            var categoryMap = new Dictionary<string, (IChildNode, string)>();
            
            foreach (var command in subcommands)
            {
                if (!categoryMap.ContainsKey(command.Key))
                    categoryMap[command.Key] = (command,
                                                command is GroupNode group
                                                    ? group
                                                     .GroupTypes
                                                     .Select(g => g.GetCustomAttribute<HelpCategoryAttribute>())
                                                     .FirstOrDefault(ha => ha is not null)?
                                                     .Name ?? Categories.Uncategorized
                                                    : ((CommandNode) command)
                                                     .GroupType
                                                     .GetCustomAttribute<HelpCategoryAttribute>()?
                                                     .Name ?? Categories.Uncategorized);
            }
            
            var categories = categoryMap
                            .GroupBy(c => c.Value.Item2)
                            .OrderBy(c => Categories.Order.IndexOf(c.Key));
            
            fields.AddRange(categories.Select(c => new EmbedField("`" + c.Key + "`", c.Select(cn => $"`{cn.Key}`").Join(", "))));

            embed = new Embed
            {
                Title       = "Help",
                Description = "Wanna see more information? Try specifing a command!",
                Colour      = Color.DodgerBlue,
                Fields      = fields
            };
        }
        else
        {
            var node = subcommands.First().Parent as IChildNode;

            var containsDefaultCommand = IsExecutableGroup(node, out var usage);
            
            embed = new Embed
            {
                Title = $"Help for {node.Key}:",
                Description = "Showing subcommands. \n"                            +
                              "Specify a command name to see more information.\n\n" +
                              (containsDefaultCommand ? usage : (node as CommandNode)?.Shape.Description ?? (node as GroupNode)?.Description),
                Colour = Color.DodgerBlue,
                Fields = new[]
                {
                    new EmbedField("Aliases", node.Aliases.Any() ? string.Join(", ", node.Aliases) : "None", true),
                    new EmbedField("Subcommands", commandString, true)
                }
            };
        }

        return embed;
    }

    private bool IsExecutableGroup(IChildNode node, out string usage)
    {
        usage = "";

        if (node is not GroupNode gn || gn.Parent is not {} parent) //How??
            return false;
        
        if (parent.Children.Count < 2)
            return false;
        
        var nodes = parent.Children.Where(n => n.Key == node.Key);
        
        if (nodes.Count() < 2)
            return false;

        var command = nodes.FirstOrDefault(n => n is not IParentNode) as CommandNode;

        if (command is null)
            return false; // Plugins may add on to a group.

        usage = "This group can be used like a command:\n" + GetParameterHelp(command);

        return true;
    }
    
    /// <summary>
    ///     Gets a neatly formatted parameter name for the help embed.
    /// </summary>
    /// <param name="param">The paremeter to generate a help name for.</param>
    /// <returns>The paremeter name, formatted to respect switches and options, if applicable.</returns>
    private string GetHumanFriendlyParemeterName(IParameterShape param)
    {
        return param.Parameter.GetCustomAttribute<OptionAttribute>() is { } oa 
            ? param.IsOmissible() 
                ? $"[{GetOptionString(oa)}{(oa is not SwitchAttribute ? $" <{param.Parameter.Name}>" : null)}]" 
                : $"<{GetOptionString(oa)}{(oa is not SwitchAttribute ? $" <{param.Parameter.Name}>" : null)}>" 
            : param.IsOmissible()
                ? $"[{param.HintName}]" 
                : $"<{param.HintName}>";
    }

    private string GetOptionString(OptionAttribute option)
    {
        return option switch
        {
            _ when option.ShortName is not null &&
                   option.LongName is not null => $"-{option.ShortName} OR --{option.LongName}",

            _ when option.ShortName is not null => $"-{option.ShortName}",

            _ when option.LongName is not null => $"--{option.LongName}",
            
            _ => throw new InvalidOperationException("Option must have a name!")
        };
    }
}