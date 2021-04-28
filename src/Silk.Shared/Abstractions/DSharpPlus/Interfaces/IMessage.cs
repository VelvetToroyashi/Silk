using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Silk.Shared.Abstractions.DSharpPlus.Interfaces
{
    public interface IMessage
    {
        /// <summary>
        /// The Id of the message.
        /// </summary>
        public ulong Id { get; }

        /// <summary>
        /// The Id of the guild this message belongs to, if any.
        /// </summary>
        public ulong? GuildId { get; }

        /// <summary>
        /// The channel this message was created in.
        /// </summary>
        public IChannel Channel { get; }

        /// <summary>
        /// The user that sent the message.
        /// </summary>
        public IUser Author { get; }

        /// <summary>
        /// The content of the message.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// When this message was created.
        /// </summary>
        public DateTimeOffset CreationTimestamp { get; }

        /// <summary>
        /// The message this message is a reply to.
        /// </summary>
        public IMessage? Reply { get; }

        /// <summary>
        /// Reactions added to this message.
        /// </summary>
        public IReadOnlyCollection<IEmoji> Reactions { get; }

        /// <summary>
        /// Users mentioned in this message
        /// </summary>
        public IReadOnlyCollection<IUser> MentionedUsers { get; }

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