namespace Silk.Core.Types
{
    /// <summary>
    ///     Record that represents the result of creating a tag.
    /// </summary>
    public record TagCreationResult(bool Success, string? Reason);

}