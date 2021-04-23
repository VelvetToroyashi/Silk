namespace Silk.Shared.Abstractions.DSharpPlus.Interfaces
{
    public interface IEmoji
    {
        /// <summary>
        /// The Id of this emoji.
        /// </summary>
        public ulong Id { get; init; }

        /// <summary>
        /// The name of this emoji.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Returns a true or false value of whether the emoji is in any shared guild.
        /// </summary>
        public bool IsSharedEmoji();
    }
}