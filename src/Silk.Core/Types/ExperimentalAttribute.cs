using System;
using System.Linq;
using DSharpPlus.CommandsNext;

namespace Silk.Core.Types
{
    ///<summary>
    /// Denotes this command is experimental, and may not work properly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ExperimentalAttribute : Attribute { }

    public static class ExperimentalAttributeExtensions
    {
        public static bool IsExperimental(this Command c) =>
            c.CustomAttributes.OfType<ExperimentalAttribute>().Any();
    }
}