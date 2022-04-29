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
}