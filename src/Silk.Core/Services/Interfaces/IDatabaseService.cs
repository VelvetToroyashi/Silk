using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Silk.Core.Database.Models;

namespace Silk.Core.Services.Interfaces
{
    /// <summary>
    /// Provides database abstractions for command classes, and methods for updating and retrieving
    /// <see cref="User"/> and <see cref="Guild"/>.
    /// </summary>
    public interface IDatabaseService
    {
        /// <summary>
        /// Get a <see cref="Guild"/> from the database.
        /// </summary>
        /// <returns><see cref="Guild"/>with corresponding users.</returns>
        public Task<Guild?> GetGuildAsync(ulong guildId);

        /// <summary>
        /// This method is used for initializing a guild in case it doesn't exist, in the case of caching,
        /// or any other situation that a guild needs to be used, but may not exist in the database.
        /// </summary>
        /// <param name="guildId">The id of the guild to get, or create if it doesn't exist.</param>
        /// <returns>A <see cref="Guild"/>, which will have default properties if it did not exist.</returns>
        public Task<Guild> GetOrCreateGuildAsync(ulong guildId);

        /// <summary>
        /// This method is used to update a <see cref="Guild"/>.
        /// </summary>
        /// <param name="guild">The guild to update.</param>
        public Task UpdateGuildAsync(Guild guild);

        /// <summary>
        /// Get the configuration of a <see cref="Guild"/>.
        /// </summary>
        /// <param name="guildId">The Id of the guild to retrieve a configuration for.</param>
        /// <returns>The configuration that corresponds to the Id passed in.</returns>
        public Task<GuildConfig> GetConfigAsync(ulong guildId);

        /// <summary>
        /// Update a <see cref="GuildConfig"/>.
        /// </summary>
        /// <param name="config">The configuration to update.</param>
        public Task UpdateConfigAsync(GuildConfig config);

        /// <summary>
        /// Get a <see cref="User"/> from the database.
        /// </summary>
        /// <returns>A <see cref="User"/>, or null if the user doesn't exist.</returns>
        public Task<User?> GetGuildUserAsync(ulong guildId, ulong userId);

        /// <summary>
        /// Get a <see cref="GlobalUser"/> from the database.
        /// </summary>
        /// <param name="userId">The Id of the user to retrieve.</param>
        /// <returns>A <see cref="GlobalUser"/>, or null if the user doesn't exist.</returns>
        public Task<GlobalUser?> GetGlobalUserAsync(ulong userId);

        /// <summary>
        /// Update a <see cref="User"/>
        /// </summary>
        /// <param name="user">The user to update.</param>
        /// <remarks>Remarks: This differs from <see cref="UpdateGuildUserAsync(User)"/> in the sense that
        /// the object is updated externally, and is updated in the database.</remarks>
        public Task UpdateGuildUserAsync(User user);

        /// <summary>
        /// Update a <see cref="GlobalUser"/>.
        /// </summary>
        /// <param name="user">The user to update.</param>
        public Task UpdateGlobalUserAsync(GlobalUser user);

        /// <summary>
        /// Get a <see cref="User"/> from the database. Unlike <see cref="GetGuildUserAsync"/>, the object is guarunteed to be non-null.
        /// </summary>
        /// <returns>A <see cref="User"/></returns>
        public Task<User> GetOrCreateGuildUserAsync(ulong guildId, ulong userId);

        /// <summary>
        /// Get a <see cref="GlobalUser"/> from the database.
        /// </summary>
        /// <param name="userId">The Id of the user to retreive.</param>
        /// <returns>A <see cref="GlobalUser"/>, which will have default properties if it did not exist in the databsae.</returns>
        public Task<GlobalUser> GetOrCreateGlobalUserAsync(ulong userId);

        /// <summary>
        /// Remove a user from the database. A user with infractions will not be removed from the database.
        /// </summary>
        /// <param name="user">The user to attempt to remove.</param>
        /// <returns></returns>
        public Task RemoveUserAsync(User user);

        /// <summary>
        /// Get all users that match a predicate.
        /// </summary>
        /// <param name="predicate">The expression to apply to all users</param>
        /// <returns>A collection of users that match the supplied predicate.</returns>
        public Task<IEnumerable<User>> GetAllUsersAsync(Expression<Func<User, bool>> predicate);

        /// <summary>
        /// Get all active infractions.
        /// </summary>
        /// <returns>A collection of active, temporary infractions.</returns>
        public Task<IEnumerable<Infraction>> GetActiveInfractionsAsync();


    }
}