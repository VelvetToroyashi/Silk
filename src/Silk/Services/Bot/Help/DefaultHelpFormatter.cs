using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Remora.Commands;
using Remora.Commands.Attributes;
using Remora.Commands.Extensions;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Extensions;
using Silk.Infrastructure;
using VTP.Remora.Commands.HelpSystem;

namespace Silk.Services.Bot.Help;

[PublicAPI]
public class DefaultHelpFormatter : IHelpFormatter
{
    private readonly HelpSystemOptions _options;
    
    public DefaultHelpFormatter(IOptions<HelpSystemOptions> options)
    {
        _options = options.Value;
    }
    

    public IEmbed GetCommandHelp(IChildNode command)
    {
        var sb = new StringBuilder();

        var casted = (CommandNode)command;
        var commandArray = new[] {command};
        
        AddCommandPath(sb, command);
        AddCommandAliases(sb, commandArray);
        AddCommandDescription(sb, commandArray);
        AddCommandUsage(sb, command);
        AddRequiredPermissions(sb, command);

        var embed = GetBaseEmbed() with
        {
            Title = $"Help for {command.Key}",
            Description = sb.ToString()
        };

        return embed;
    }
    
    public IEnumerable<IEmbed> GetCommandHelp(IEnumerable<IChildNode> subCommands)
    {
        Embed embed;
        
        var descriptionBuilder = new StringBuilder();

        var subCommandArray = subCommands.ToArray();
        
        if (subCommandArray.Length is 1)
        {
            if (subCommandArray.Single() is not IParentNode pn)
            {
                yield return GetCommandHelp(subCommandArray.Single());
                yield break;
            }
            else
            {
                var groupedChildren = pn.Children.GroupBy(x => x.Key);

                AddCommandPath(descriptionBuilder, (IChildNode)pn);
                AddCommandAliases(descriptionBuilder, subCommandArray);
                AddCommandDescription(descriptionBuilder, subCommandArray);
                AddSubCommands(groupedChildren);
                AddRequiredPermissions(descriptionBuilder, (IChildNode)pn);
                
                embed = GetBaseEmbed() with
                {
                    Title = $"Showing sub-command help for {subCommandArray.Single().Key}",
                    Description = descriptionBuilder.ToString()
                };

                yield return embed;
                yield break;
            }
        }
        
        // We're looking at a group's children; recall with the group
        // since we have special handling for this case.
        if (subCommandArray.DistinctBy(sc => sc.Key).Count() == subCommandArray.Length)
        {
            yield return GetCommandHelp(new[] { (IChildNode)subCommandArray.First().Parent }).Single();
            yield break;
        }
        
        if (!subCommandArray.OfType<IParentNode>().Any())
        {
            var sca = subCommandArray.ToArray();
            
            for (int i = 0; i < sca.Length; i++)
                yield return (GetCommandHelp(sca[i]) as Embed)! with { Title = $"Help for {sca[0].Key} (overload {i + 1} of {sca.Length})" };

            yield break;
        }
        
        // If we need to deal with overloaded groups, it's actually pretty simple.
        // var children = subCommands.OfType<IParentNode>().SelectMany(x => x.Children);
        // var parent = subCommands.OfType<IParentNode>().First();
        // Then, just use the children as you would normally. The reaosn this isn't done
        // by default is because it's somewhat niche? But the code is there in case changes
        // need to be made. There's also the performance impact of re-iterating more than we
        // have to, but we're using LINQ, so allocations > speed anyways.
        var group = (GroupNode)subCommandArray.First(sc => sc is IParentNode);

        // This makes the assumption that there are no overloaded groups,
        // which is impossible to do without backtracking anwyay.
        var executable = subCommandArray.Where(sc => sc is not IParentNode).Cast<CommandNode>();
        
        var gsc = group.Children.GroupBy(x => x.Key);
        
        AddCommandPath(descriptionBuilder, group);
        AddCommandAliases(descriptionBuilder, subCommandArray);
        AddCommandDescription(descriptionBuilder, subCommandArray);
        AddGroupCommandUsage(descriptionBuilder, executable);
        AddSubCommands(gsc);
        AddRequiredPermissions(descriptionBuilder, group);
        
        embed = GetBaseEmbed() with
        {
            Title = $"Showing sub-command help for {group.Key}",
            Description = descriptionBuilder.ToString()
        };
        
        yield return embed;

        yield break;

        void AddSubCommands(IEnumerable<IGrouping<string, IChildNode>> grouped)
        {
            descriptionBuilder.AppendLine("**Sub-commands**");

            foreach (var command in grouped)
            {
                if (command.Count() > 1 && command.Any(sc => sc is IParentNode))
                    descriptionBuilder.AppendLine($"`{command.Key}*`");
                else
                    descriptionBuilder.AppendLine($"`{command.Key}`");
            }
            
            descriptionBuilder.AppendLine();
        }
    }
    
    public IEnumerable<IEmbed> GetTopLevelHelpEmbeds(IEnumerable<IGrouping<string, IChildNode>> commands)
    {
        var sb = new StringBuilder();
        
        if (!_options.CommandCategories.Any())
        {
            var sorted = commands.OrderBy(x => x.Key);
            
            foreach (var group in sorted)
            {
                if (group.Count() is 1 || group.All(g => g is not IParentNode))
                    sb.Append($"`{group.Key}` ");
                else
                    sb.Append($"`{group.Key}*` ");
            }
        }
        else
        {
            // We're gonna need to loop over this a few times.
            var commandArray = commands.ToArray();

            var categorized = new Dictionary<string, List<IGrouping<string, IChildNode>>>();

            foreach (var group in commandArray)
            {
                string category = "Uncategorized";
                
                foreach (var command in group)
                {
                    var categoryAttribute = command.GetAttributeFromTree<CategoryAttribute>();

                    if (categoryAttribute is null) 
                        continue;

                    var categoryName = _options.CommandCategories.FirstOrDefault(c => c.Equals(categoryAttribute.Category, StringComparison.OrdinalIgnoreCase));
                    
                    if (categoryName is null) 
                        continue;
                    
                    category = categoryName;
                    break;
                }
                
                if (!categorized.ContainsKey(category))
                    categorized.Add(category, new());
                
                categorized[category].Add(group);
            }

            foreach (var category in categorized.Skip(1).OrderBy(c => _options.CommandCategories.IndexOf(c.Key)).Append(categorized.First()))
            {
                sb.AppendLine($"**`{category.Key}`**");
                
                foreach (var group in category.Value)
                {
                    if (group.Count() is 1 || group.All(g => g is not IParentNode))
                        sb.Append($"`{group.Key}` ");
                    else
                        sb.Append($"`{group.Key}*` ");
                }
                
                sb.AppendLine()
                  .AppendLine();
            }
        }

        var embed = GetBaseEmbed() with
        {
            Title = "All Commands",
            Description = sb.ToString(),
            Footer = new EmbedFooter("Specify a command for more information. Commands with \"*\" are groups that can be used like commands."),
        };
        
        yield return embed;
    }

    private Embed GetBaseEmbed() => new() { Colour = Color.DodgerBlue };

    private void AddGroupCommandUsage(StringBuilder builder, IEnumerable<IChildNode> overloads)
    {
        var casted = overloads.Cast<CommandNode>().ToArray();
        
        builder.AppendLine("**Usage**");
        
        builder.Append($"This group can be executed like a command");

        if (casted.Any(ol => !ol.Shape.Parameters.Any()))
            builder.Append(" without parameters");
        
        builder.AppendLine(".");

        foreach (var overload in casted)
        {
            if (!overload.Shape.Parameters.Any())
                continue;

            builder.AppendLine();
            
            var localBuilder = new StringBuilder();
            
            localBuilder.Append('`');

            AddCommandPath(localBuilder, overload, false);
            
            localBuilder.Append(' ');

            foreach (var parameter in overload.Shape.Parameters)
            {
                localBuilder.Append(parameter.IsOmissible() ? "[" : "<");

                char? shortName = null;
                string? longName = null;

                var named = false;
                var isSwitch = false;

                if (parameter.Parameter.GetCustomAttribute<SwitchAttribute>() is { } sa)
                {
                    named = true;
                    isSwitch = true;

                    shortName = sa.ShortName;
                    longName = sa.LongName;
                }

                if (parameter.Parameter.GetCustomAttribute<OptionAttribute>() is { } oa)
                {
                    named = true;

                    shortName = oa.ShortName;
                    longName = oa.LongName;
                }

                if (named)
                {
                    if (shortName is not null && longName is not null)
                    {
                        localBuilder.Append($"-{shortName}/--{longName}");
                    }
                    else
                    {
                        localBuilder.Append(shortName is not null ? $"-{shortName}" : $"--{longName}");
                    }

                    if (!isSwitch)
                        localBuilder.Append(' ');
                }

                if (!isSwitch)
                    localBuilder.Append(parameter.Parameter.Name);

                localBuilder.Append(parameter.IsOmissible() ? ']' : '>');

                localBuilder.Append(' ');
            }
            
            localBuilder[^1] = '`';
            
            builder.AppendLine(localBuilder.ToString());


            localBuilder.Clear();
        }
        
        builder.AppendLine();
    }
    
    private void AddCommandUsage(StringBuilder builder, IChildNode command)
    {
        var cn = (CommandNode)command;

        builder.AppendLine("**Usage**");

        if (!cn.Shape.Parameters.Any())
        {
            builder.AppendLine("This command can be used without any parameters.");
            return;
        }
        
        foreach (var parameter in cn.Shape.Parameters)
        {
            builder.Append(parameter.IsOmissible() ? "`[" : "`<");

            char? shortName = null;
            string? longName = null;

            var named = false;
            var isSwitch = false;

            if (parameter.Parameter.GetCustomAttribute<SwitchAttribute>() is { } sa)
            {
                named = true;
                isSwitch = true;

                shortName = sa.ShortName;
                longName = sa.LongName;
            }
            
            if (parameter.Parameter.GetCustomAttribute<OptionAttribute>() is { } oa)
            {
                named = true;

                shortName = oa.ShortName;
                longName = oa.LongName;
            }

            if (named)
            {
                if (shortName is not null && longName is not null)
                {
                    builder.Append($"-{shortName}/--{longName}");
                }
                else
                {
                    if (shortName is not null)
                        builder.Append($"-{shortName}");
                    else
                        builder.Append($"--{longName}");
                }
                
                if (!isSwitch)
                    builder.Append(' ');
            }

            if (!isSwitch)
                builder.Append(parameter.Parameter.Name);

            builder.Append(parameter.IsOmissible() ? "]`" : ">`");

            builder.AppendLine($" {(string.IsNullOrEmpty(parameter.Description) ? "No description" : parameter.Description)}");
            builder.AppendLine();
        }
    }

    private void AddCommandAliases(StringBuilder builder, IEnumerable<IChildNode> commands)
    {
        var aliases = commands.SelectMany(c => c.Aliases);

        builder.AppendLine("**Aliases**");

        if (!aliases.Any())
        {
            builder.AppendLine("No aliases set.");
            builder.AppendLine();
            return;
        }
        
        builder.AppendLine(string.Join(", ", aliases));
        builder.AppendLine();
    }

    private void AddRequiredPermissions(StringBuilder builder, IChildNode node)
    {
        RequireDiscordPermissionAttribute? rpa = null;

        if (node is GroupNode gn)
        {
            if (!gn.Children.Any())
                return;

            if (gn.Children.All(child => child is IParentNode))
                return; // Don't feel like traversing the tree deeper, so just bail.
            
            rpa = (gn.Children.First(child => child is CommandNode) as CommandNode)!.FindCustomAttributeOnLocalTree<RequireDiscordPermissionAttribute>();
        }

        if (node is CommandNode cn)
            rpa = cn.FindCustomAttributeOnLocalTree<RequireDiscordPermissionAttribute>();
        
        if (rpa is null)
            return;

        builder.AppendLine($"This command requires the following permissions: {string.Join(", ", rpa.Permissions)}");
        
        builder.AppendLine();
    }

    private void AddCommandPath(StringBuilder builder, IChildNode node, bool appendPath = true)
    {
        var path = new List<string?>();
        
        IParentNode? parent = node.Parent;

        path.Add(node.Key);
        path.Add((parent as IChildNode)?.Key);
        
        do
        {
            parent = (parent as IChildNode)?.Parent;
            
            path.Add((parent as IChildNode)?.Key);
        } 
        while (parent is not (null or RootNode));
        
        path.Reverse();

        if (!appendPath)
        {
            builder.Append(string.Join(' ', path).Trim());
        }
        else
        {
            builder.AppendLine($"**Path**\n`{string.Join(" ", path).Trim()}`");
            builder.AppendLine();
        }

    }

    private void AddCommandDescription(StringBuilder builder, IEnumerable<IChildNode> nodes)
    {
        builder.AppendLine("**Description**");
        
        if (nodes.Count() is 1)
        {
            if (nodes.First() is CommandNode cn)
            {
                if (string.IsNullOrEmpty(cn.Shape.Description))
                    builder.AppendLine("No description set.");
                else 
                    builder.AppendLine(cn.Shape.Description);
            }
            else if (nodes.First() is GroupNode gn)
            {
                builder.AppendLine(gn.GetDescription() ?? Constants.DefaultDescription);
            }
        }
        else
        {
            if (nodes.FirstOrDefault(n => n is GroupNode) is GroupNode fgn)
            {
                var description = fgn.GetDescription() ??
                                  nodes.OfType<CommandNode>()
                                      .FirstOrDefault(cn => cn.GetDescription() is not null)?.Shape.Description ??
                                  Constants.DefaultDescription;
           
                builder.AppendLine(description);
            }
            else
            {
                var description = nodes
                                      .Cast<CommandNode>()
                                      .FirstOrDefault(cn => cn.GetDescription() is not null)?
                                      .Shape
                                      .Description ??
                                  Constants.DefaultDescription;
            
                builder.AppendLine(description);
            }
        }
        
        builder.AppendLine();
    }
}