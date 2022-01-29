using System;

namespace Silk.Utilities.HelpFormatter;

/// <summary>
///     Marks this class as being part of a command category with a specific name
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class HelpCategoryAttribute : Attribute
{

    public HelpCategoryAttribute(string name) => Name = name;
    public string Name { get; }
}