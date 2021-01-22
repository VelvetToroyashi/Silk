using System;

namespace Silk.Core.Utilities
{
    ///<summary>
    /// Denotes this command is expiremental, and may not work properly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ExpirementalAttribute : Attribute { }
}