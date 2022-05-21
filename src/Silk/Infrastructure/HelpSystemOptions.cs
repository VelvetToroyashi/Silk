using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Silk.Infrastructure;

/// <summary>
/// Options related to the command help system.
/// </summary>
[ExcludeFromCodeCoverage]
public class HelpSystemOptions
{
    /// <summary>
    /// The tree to search when looking for commands.
    /// </summary>
    public string? TreeName { get; set; }
    
    /// <summary>
    /// Whether or not to show commands regardless of if their conditions have been met.
    /// </summary>
    public bool AlwaysShowCommands { get; set; }
    
    /// <summary>
    /// Gets a list of named groups to help categorize commands.
    /// If any are specified, commands will be grouped by the cateogory listed on command module.
    /// </summary>
    public List<string> CommandCategories { get; } = new();
}