using System;

namespace SilkBot.Utilities
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
    public class HelpAttribute : Attribute
    {


    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
    public class HelpDescriptionAttribute : Attribute
    {
        private readonly string description;

        private string[] exampleUsages;

        public HelpDescriptionAttribute(string desc, params string[] usages)
        {
            description = desc;
            exampleUsages = usages;
        }

    }
}
