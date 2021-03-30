using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Silk.Shared.Abstractions.DSharpPlus.Interfaces
{
    /// <summary>
    /// Represents a mockable abstraction of a <see cref="DiscordChannel"/>.
    /// </summary>
    public interface IChannel
    {
        /// <summary>
        /// The id of the current channel.
        /// </summary>
        public ulong Id { get; init; }

        /// <summary>
        /// What guild this channel belongs to, if any.
        /// </summary>
        public IGuild? Guild { get; init; }

        /// <summary>
        /// The messages sent in this channel.
        /// </summary>
        public IReadOnlyList<IMessage> Messages { get; init; }

        /// <summary>
        /// Sends an <see cref="IMessage"/> to the channel.
        /// </summary>
        /// <param name="message">The message to send to the channel.</param>
        /// <returns>The sent <see cref="IMessage"/>.</returns>
        public Task<IMessage> SendAsync(IMessage message);

        /// <summary>
        /// Gets a specific message from the channel.
        /// </summary>
        /// <param name="id">The Id of the message to retrieve.</param>
        /// <returns>An <see cref="IMessage"/> if the message exists, otherwise null.</returns>
        public Task<IMessage?> GetMessageAsync(ulong id);
    }
}