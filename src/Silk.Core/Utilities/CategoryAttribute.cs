using System;

namespace Silk.Core.Utilities
{
    /// <summary>
    /// Marks this class as being part of a command category with a specific name
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CategoryAttribute : Attribute
    {
        public string Name { get; }

        public CategoryAttribute(string name) => Name = name;

    }
}