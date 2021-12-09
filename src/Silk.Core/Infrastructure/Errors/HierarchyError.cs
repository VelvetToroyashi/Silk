using Remora.Results;

namespace Silk.Core.Errors;

/// <summary>
///     Represents an error that occurs when hiearchy prevents an action from being performed.
/// </summary>
/// <param name="Message">The message of the error.</param>
public record HierarchyError(string Message) : IResultError;