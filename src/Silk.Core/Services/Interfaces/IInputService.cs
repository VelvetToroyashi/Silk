using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Silk.Core.Services.Interfaces
{
    public interface IInputService
    {
        /// <summary>
        /// Gets input from a specific user.
        /// </summary>
        /// <param name="userId">The Id of the user to listen for input from.</param>
        /// <param name="channelId">The Id of the channel to listen for input from.</param>
        /// <param name="guildId">The Id of the guild to listen for input from, or null, if it is a DM.</param>
        /// <param name="timeOut">Optional override for the wait period before timing out.</param>
        /// <returns>The user's input, or null if it timed out.</returns>
        public Task<string?> GetStringInputAsync(ulong userId, ulong channelId, ulong? guildId = null, TimeSpan? timeOut = null);

        /// <summary>
        /// Gets a message via text input, and attempts to parse a true or false value out of it.
        /// </summary>
        /// <param name="userId">The Id of the user to listen for input from.</param>
        /// <param name="channelId">The Id of the channel to listen for input from.</param>
        /// <param name="guildId">The Id of the guild to listen for input from, or null, if it is a DM.</param>
        /// <param name="timeOut">Optional override for the wait period before timing out.</param>
        /// <returns>A true or false value, or null, if it timed out.</returns>
        public Task<bool?> GetBoolInputFromMessageAsync(ulong userId, ulong channelId, ulong? guildId = null, TimeSpan? timeOut = null);
        public Task<DiscordEmoji> GetReactionInputAsync(ulong userId, ulong channelId, ulong? guildId = null, TimeSpan? timeOut = null);



    }
}