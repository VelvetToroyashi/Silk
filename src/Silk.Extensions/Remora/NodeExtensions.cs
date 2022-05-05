using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Remora.Commands;
using Remora.Commands.Trees.Nodes;

namespace VTP.Remora.Commands.HelpSystem;


[ExcludeFromCodeCoverage]
public static class GroupNodeExtensions
{
    /// <summary>
    /// Returns the description of a <see cref="GroupNode"/> if set, otherwise <c>null</c>.
    /// </summary>
    /// <param name="gn">The group node to get the description from.</param>
    /// <returns>The description of the group node if set, otherwise <c>null</c>.</returns>
    public static string? GetDescription(this GroupNode gn)
    {
        if (string.Equals("No description set.", gn.Description, StringComparison.OrdinalIgnoreCase))
            return null;
        
        return string.IsNullOrEmpty(gn.Description) ? null : gn.Description;

    }
    
    /// <summary>
    /// Returns the description of a <see cref="GroupNode"/> if set, otherwise <c>null</c>.
    /// </summary>
    /// <param name="cn">The command node to get the description from.</param>
    /// <returns>The description of the command node if set, otherwise <c>null</c>.</returns>
    public static string? GetDescription(this CommandNode cn)
    {
        if (string.Equals(Constants.DefaultDescription, cn.Shape.Description, StringComparison.OrdinalIgnoreCase))
            return null;
        
        return string.IsNullOrEmpty(cn.Shape.Description) ? null : cn.Shape.Description;
    }

    /// <summary>
    /// Attempts to gather an attribute from a given <see cref="IChildNode"/>, traversing only it's direct anscestors.
    /// </summary>
    /// <param name="startingNode">The node to start traversing from.</param>
    /// <typeparam name="TAttribute">The attribute to find on the tree.</typeparam>
    /// <returns>The found attribute if any, otherwise null.</returns>
    public static TAttribute? GetAttributeFromTree<TAttribute>(this IChildNode startingNode) where TAttribute : Attribute
    {
        IChildNode? parent = null;
        TAttribute? returnAttribute = null;
        
        if (startingNode is CommandNode cn)
        {
            if ((cn.CommandMethod.GetCustomAttribute<TAttribute>() ?? cn.GroupType.GetCustomAttribute<TAttribute>()) is { } commandAttribute)
                return commandAttribute;
        }
        else if (startingNode is GroupNode gn)
        {
            if (gn.GroupTypes.Select(gt => gt.GetCustomAttribute<TAttribute>()).FirstOrDefault(gt => gt is not null) is { } groupAttribute)
                return groupAttribute;
        }
        
        do
        {
            parent = (parent?.Parent ?? startingNode.Parent) as IChildNode;

            var attributes = (parent as GroupNode)?.GroupTypes.SelectMany(gt => gt.GetCustomAttributes<TAttribute>());
            
            if (attributes?.FirstOrDefault() is {} attribute)
                returnAttribute = attribute;
        }
        while (parent?.Parent is GroupNode && returnAttribute is null);
        
        return returnAttribute;
    }

    public static IEnumerable<TAttribute> GetAttribtuesFromTree<TAttribute>(this IChildNode startingNode, bool returnAnscestors) where TAttribute : Attribute
    {
        IChildNode? parent = null;

        switch (startingNode)
        {
            case CommandNode cn:
            {
                if ((cn.CommandMethod.GetCustomAttribute<TAttribute>() ?? cn.GroupType.GetCustomAttribute<TAttribute>()) is { } commandAttribute)
                {
                    yield return commandAttribute;
                
                    if (!returnAnscestors)
                        yield break;
                }
                break;
            }
            case GroupNode gn:
            {
                var attributes = gn.GroupTypes.Select(gt => gt.GetCustomAttribute<TAttribute>()).Where(att => att is not null);
            
                foreach (var attribute in attributes)
                    yield return attribute!;
            
                if (!returnAnscestors)
                    yield break;
                break;
            }
        }
        
        do
        {
            parent = (parent?.Parent ?? startingNode.Parent) as IChildNode;

            var attributes = (parent as GroupNode)?.GroupTypes.SelectMany(gt => gt.GetCustomAttributes<TAttribute>());

            if (attributes?.FirstOrDefault() is not { } attribute) 
                continue;
            
            yield return attribute;
                
            if (!returnAnscestors)
                yield break;
        }
        while (parent?.Parent is GroupNode);
    }
}