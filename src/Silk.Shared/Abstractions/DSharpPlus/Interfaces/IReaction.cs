using System.Threading.Tasks;

namespace Silk.Shared.Abstractions.DSharpPlus.Interfaces
{
    public interface IReaction
    {
        /// <summary>
        /// The emoji added to the message.
        /// </summary>
        public IEmoji Emoji { get; }

        /// <summary>
        /// The user that added this reaction.
        /// </summary>
        public ulong UserId { get; }

        /// <summary>
        /// Deletes this reaction. 
        /// </summary>
        public Task DeleteAsync();
    }
}