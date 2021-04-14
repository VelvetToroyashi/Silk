using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Silk.Shared.Abstractions.DSharpPlus.Interfaces
{
    public interface IMessage
    {
        public ulong Id { get; }
        public ulong? GuildId { get; }
        public IUser Author { get; }
        public string Content { get; }
        public DateTimeOffset Timestamp { get; }
        public IMessage? Reply { get; }
        public IReadOnlyCollection<IEmoji> Reactions { get; }

        /// <summary>
        /// Creates a reaction on this message.
        /// </summary>
        /// <param name="emojiId">The Id of th emoji to add.</param>
        public Task CreateReactionAsync(ulong emojiId);
    }
}