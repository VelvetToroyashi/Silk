using Silk.Core.Data.Models;

namespace Silk.Core.Services.Interfaces
{
    /// <summary>
    /// Provides methods for updating and retrieving a
    /// <see cref="Guild"/>'s <seealso cref="string"/> prefix, which is used for command recognition.
    /// </summary>
    public interface IPrefixCacheService : ICacheService
    {
        /// <summary>
        /// Retrieve the corresponding prefix for a <see cref="Guild"/>.
        /// </summary>
        /// <param name="guildId">The Id of the guild to retrieve the prefix for.</param>
        /// <returns>The prefix configured for the guild.</returns>
        public string RetrievePrefix(ulong? guildId);

        /// <summary>
        /// Update a prefix for a <see cref="Guild"/>.
        /// </summary>
        /// <param name="guildId">The Id of the guild to update.</param>
        /// <param name="prefix">The prefix to assign to the guild.</param>
        public void UpdatePrefix(ulong guildId, string prefix);

    }
}