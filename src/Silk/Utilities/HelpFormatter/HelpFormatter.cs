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
using Remora.Discord.Commands.Attributes;
using Silk.Extensions;

namespace Silk.Utilities.HelpFormatter;

public class HelpFormatter : IHelpFormatter
{
    /// <inheritdoc />
    public IEnumerable<IEmbed> GetCommandHelpEmbeds(OneOf<CommandNode, IReadOnlyList<CommandNode>> command)
    {
        if (command.TryPickT0(out var standalone, out var overloads))
        {
            string aliases = standalone.Aliases.Any() ? string.Join(", ", standalone.Aliases) + '\n' : "None";

            var embed = new Embed
            {
                //Title = command.Key,
                Description = $"**Command** - {standalone.Key}\n"                  +
                              $"**Aliases** - {aliases}\n"                         +
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

            IOrderedEnumerable<IGrouping<string?, IChildNode>>? categories = subcommands
                .GroupBy(x => x is CommandNode cn
                             ? cn.GroupType.GetCustomAttribute<HelpCategoryAttribute>()?.Name
                             : ((x as IParentNode).Children.FirstOrDefault() as CommandNode)?
                              .GroupType.GetCustomAttribute<HelpCategoryAttribute>()
                             ?.Name)
                .OrderBy(x => Categories.Order.IndexOf(x.Key ?? Categories.Uncategorized));

            fields
               .AddRange(categories
                            .Select(c => new EmbedField(c.Key ?? Categories.Uncategorized, c
                                                                                          .Select(cn => $"`{cn.Key}`")
                                                                                          .Join(", "))));

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
            IChildNode node;
            if (subcommands.Count() is 1)
                node  = subcommands.Single().Parent as IChildNode;
            else node = subcommands.First();

            embed = new Embed
            {
                Title = $"Help for {node!.Key}:",
                Description = "Showing subcommands. \n" +
                              "Specify a command name to see more information. \n",
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