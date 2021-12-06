using Remora.Rest.Core;
using Remora.Results;

namespace Silk.Core.Errors
{
    /// <summary>
    /// Represents an error that occurs when hiearchy prevents an action from being performed.
    /// </summary>
    /// <param name="Message">The message of the error.</param>
    /// <param name="Target">The target that cannot be acted upon.</param>
    /// <param name="Difference">The difference in hierarchy. This is optional.</param>
    public record HierarchyError(string Message, Snowflake Target, int? Difference = null) : IResultError;
}