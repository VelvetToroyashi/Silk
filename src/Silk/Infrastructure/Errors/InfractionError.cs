using Remora.Results;

namespace Silk.Errors;

/// <summary>
///     There was an error while handling an infraction, but the infraction was still processed.
/// </summary>
/// <param name="Message">The user-friendly message of what went wrong.</param>
/// <param name="Inner">The inner result.</param>
public record InfractionError(string Message, Result Inner) : ResultError(Message);