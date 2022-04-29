using System;
using System.Diagnostics.CodeAnalysis;
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
        
        if (string.IsNullOrEmpty(gn.Description))
            return null;
        
        return gn.Description;
    }
}