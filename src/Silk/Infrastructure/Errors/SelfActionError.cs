using Remora.Results;

namespace Silk.Errors;

/// <inheritdoc cref="ResultError"/>
/// <summary>
/// An error that represents an action was attempting that targets either the current user, or the bot itself.
/// </summary>
public record SelfActionError(string Message) : ResultError(Message);
