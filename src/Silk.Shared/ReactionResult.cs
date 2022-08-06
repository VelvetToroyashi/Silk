using OneOf;
using Remora.Rest.Core;

namespace Silk.Shared;

/// <summary>
/// A return type that can be used to add a reaction to the source message.
/// </summary>
/// <param name="Reaction">The ID of the reaction, or its unicode representation.</param>
/// <param name="Message">The optional message to send.</param>
public record ReactionResult(OneOf<Snowflake, ulong, string> Reaction, Optional<string> Message = default);