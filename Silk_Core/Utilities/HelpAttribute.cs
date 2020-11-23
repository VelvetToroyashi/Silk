using System;

namespace SilkBot.Utilities
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
    public class HelpAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
    public class HelpDescriptionAttribute : Attribute
    {
        public readonly string Description;

        public string[] ExampleUsages;

        public HelpDescriptionAttribute(string desc, params string[] usages)
        {
            Description = desc;
            ExampleUsages = usages;
        }
    }
}