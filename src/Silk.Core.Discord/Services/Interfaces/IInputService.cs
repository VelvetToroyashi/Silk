using System;
using System.Threading.Tasks;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Core.Discord.Services.Interfaces
{
    public interface IInputService
    {
        /// <summary>
        ///     Gets input from a specific user.
        /// </summary>
        /// <param name="userId">The Id of the user to listen for input from.</param>
        /// <param name="channelId">The Id of the channel to listen for input from.</param>
        /// <param name="guildId">The Id of the guild to listen for input from, or null, if it is a DM.</param>
        /// <param name="timeOut">Optional override for the wait period before timing out.</param>
        /// <returns>The user's input, or null if it timed out.</returns>
        public Task<string?> GetStringInputAsync(ulong userId, ulong channelId, ulong? guildId = null, TimeSpan? timeOut = null);

        /// <summary>
        ///     Gets a reaction from a specific message.
        /// </summary>
        /// <param name="userId">The Id of the user to get a reaction from.</param>
        /// <param name="channelId">The Id of the channel to wait for a reaction in.</param>
        /// <param name="messageId">The Id of the message to wait for a message for.</param>
        /// <param name="guildId">The Id of the guild the channel belongs to, or null if it is a DM.</param>
        /// <param name="timeOut">Optional override for the wait period before timing out.</param>
        /// <returns>The emoji the user reacted with, or null if it timed out.</returns>
        public Task<IReaction?> GetReactionInputAsync(ulong userId, ulong channelId, ulong messageId, ulong? guildId = null, TimeSpan? timeOut = null);

        public Task<IChannel?> GetChannelAsync(ulong userId, ulong channelId, ulong guildId, TimeSpan? timeOut = null);

        public Task<bool?> GetConfirmationAsync(IMessage message, ulong userId, TimeSpan? timeOut = null);
        //public Task<ulong?> GetUlongIdInputAsync(ulong userId, ulong channelId, ulong? guildId = null, TimeSpan? timeOut = null);
    }
}