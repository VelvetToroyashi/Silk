using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Remora.Commands.Attributes;
using Remora.Commands.Signatures;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Silk.Extensions;

namespace Silk.Core.Utilities.HelpFormatter
{
    public class HelpFormatter : IHelpFormatter
    {
	    /// <inheritdoc />
	    public IEmbed GetHelpEmbed(CommandNode command)
        {
            string aliases = command.Aliases.Any() ? string.Join(", ", command.Aliases) + '\n' : "None";
            
            var parameterHelp = 
                    string.Join('\n', command
                   .Shape
                   .Parameters
                   .Select(p => $"`{GetHumanFriendlyParemeterName(p)}` - {p.Description}"));
            
            var embed = new Embed
            {
                //Title = command.Key,
                Description = $"**Command** - {command.Key}\n"             +
                              $"**Aliases** - {aliases}\n"                 +
                              $"**Usage**\n {parameterHelp}\n\n"           +
                              $"**Description** - {command.Shape.Description}", //command.Shape.Description + usage + parameterHelp,
                Colour = Color.DodgerBlue
            };

            return embed;
        }

	    /// <inheritdoc />
	    public IEmbed GetHelpEmbed(IEnumerable<IChildNode> subcommands)
        {
            IEmbed embed;
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
                    Title = "Help",
                    Description = "Wanna see more information? Try specifing a command!",
                    Colour = Color.DodgerBlue,
                    Fields = fields
                };
            }
            else
            {
                IChildNode node;
                if (subcommands.Count() is 1)
                    node = subcommands.Single().Parent as IChildNode;
                else node = subcommands.First();
                
                    embed = new Embed
                {
                    Title = $"Help for {node!.Key}:",
                    Description = "Showing subcommands. \n"                            +
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
        /// Gets the 'usage' of a command, formatting it's paremeters 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private string GetUsage(CommandNode command)
        {
            return string.Join('\n', command.Shape.Parameters.Select(GetHumanFriendlyParemeterName));
        }

        /// <summary>
        /// Gets a neatly formatted parameter name for the help embed.
        /// </summary>
        /// <param name="param">The paremeter to generate a help name for.</param>
        /// <returns>The paremeter name, formatted to respect switches and options, if applicable.</returns>
        private string GetHumanFriendlyParemeterName(IParameterShape param)
        {
            var attributes = param.Parameter.CustomAttributes;

            string hintName = param.HintName.Length > 1 ? "--" + param.HintName : "-" + param.HintName;
                
            if (attributes.Any(a => a.AttributeType == typeof(SwitchAttribute)))
            {
                return param.IsOmissible() ? $"[{hintName}]" : $"<--{hintName}>";
            }
            else if (attributes.Any(a => a.AttributeType == typeof(OptionAttribute)))
            {
                return param.IsOmissible() ? $"[--{param.HintName} <{param.Parameter.Name}>]" : $"<--{param.HintName} <{param.Parameter.Name}>>";
            }

            return param.IsOmissible() ? $"[{param.HintName}]" : $"<{param.HintName}>";
        }
    }
}