using Remora.Results;

namespace Silk.Errors;

/// <summary>
///     Represents an error that occurs when hierarchy prevents an action from being performed.
/// </summary>
/// <param name="Message">The message of the error.</param>
public record HierarchyError(string Message) : IResultError;