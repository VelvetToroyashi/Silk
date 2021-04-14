using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Silk.Shared.Abstractions.DSharpPlus.Interfaces
{
    /// <summary>
    ///     Represents a mockable abstraction of a <see cref="DiscordChannel" />.
    /// </summary>
    public interface IChannel
    {
        /// <summary>
        ///     The id of the current channel.
        /// </summary>
        public ulong Id { get; init; }

        /// <summary>
        ///     Gets a specific message from the channel.
        /// </summary>
        /// <param name="id">The Id of the message to retrieve.</param>
        /// <returns>An <see cref="IMessage" /> if the message exists, otherwise null.</returns>
        public Task<IMessage?> GetMessageAsync(ulong id);

        public string Mention => $"<#{Id}>";
    }
}