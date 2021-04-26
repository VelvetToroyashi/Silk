using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Silk.Shared.Abstractions.DSharpPlus.Interfaces
{
    public interface IMessage
    {
        public ulong Id { get; }
        public ulong? GuildId { get; }
        public ulong ChannelId { get; }
        public IUser Author { get; }
        public string Content { get; }
        public DateTimeOffset Timestamp { get; }
        public IMessage? Reply { get; }
        public IReadOnlyCollection<IEmoji> Reactions { get; }

        /// <summary>
        /// Creates a reaction on this message.
        /// </summary>
        /// <param name="emojiId">The Id of the emoji to add.</param>
        public Task CreateReactionAsync(ulong emojiId);

        /// <summary>
        /// Creates a reaction on this message.
        /// </summary>
        /// <param name="emoji">The emoji to add.</param>
        public Task CreateReactionAsync(IEmoji emoji);

        /// <summary>
        /// Removes all the reactions from the message.
        /// </summary>
        public Task RemoveReactionsAsync();

        /// <summary>
        /// Deletes a message.
        /// </summary>
        public Task DeleteAsync();

        /// <summary>
        /// Edits a message's content.
        /// </summary>
        public Task EditAsync(string content);
    }
}