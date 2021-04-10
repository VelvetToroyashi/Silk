using System;

namespace Silk.Core.Discord.Utilities.HelpFormatter
{
    /// <summary>
    ///     Marks this class as being part of a command category with a specific name
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CategoryAttribute : Attribute
    {

        public CategoryAttribute(string name)
        {
            Name = name;
        }
        public string Name { get; }
    }
}