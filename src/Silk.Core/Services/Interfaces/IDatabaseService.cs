using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Silk.Core.Database.Models;

namespace Silk.Core.Services.Interfaces
{
    /// <summary>
    /// Provides database abstractions for command classes, and methods for updating and retrieving
    /// <see cref="Silk.Core.Database.Models.UserModel"/> and <see cref="Silk.Core.Database.Models.GuildModel"/>.
    /// </summary>
    public interface IDatabaseService
    {
        /// <summary>
        /// Get a <see cref="GuildModel"/> from the database.
        /// </summary>
        /// <returns><see cref="GuildModel"/>with corresponding users.</returns>
        public Task<GuildModel?> GetGuildAsync(ulong guildId);

        /// <summary>
        /// This method is used for initializing a guild in case it doesn't exist, in the case of caching,
        /// or any other situation that a guild needs to be used, but may not exist in the database.
        /// </summary>
        /// <param name="guildId">The id of the guild to get, or create if it doesn't exist.</param>
        /// <returns>A <see cref="GuildModel"/>, which will have default properties if it did not exist.</returns>
        public Task<GuildModel> GetOrCreateGuildAsync(ulong guildId);
        
        /// <summary>
        /// This method is used to update a <see cref="GuildModel"/>.
        /// </summary>
        /// <param name="guild">The guild to update.</param>
        public Task UpdateGuildAsync(GuildModel guild);

        /// <summary>
        /// Get the configuration of a <see cref="GuildModel"/>.
        /// </summary>
        /// <param name="configId">The Id of the guild to retrieve a configuration for.</param>
        /// <returns>The configuration that corresponds to the Id passed in.</returns>
        public Task<GuildConfigModel> GetConfigAsync(ulong configId);

        
        /// <summary>
        /// Get a <see cref="UserModel"/> from the database.
        /// </summary>
        /// <returns>A <see cref="UserModel"/>, or null if the user doesn't exist.</returns>
        public Task<UserModel?> GetGuildUserAsync(ulong guildId, ulong userId);

        /// <summary>
        /// Get a <see cref="GlobalUserModel"/> from the database.
        /// </summary>
        /// <param name="userId">The Id of the user to retrieve.</param>
        /// <returns>A <see cref="GlobalUserModel"/>, or null if the user doesn't exist.</returns>
        public Task<GlobalUserModel?> GetGlobalUserAsync(ulong userId);
        
        /// <summary>
        /// Update a <see cref="UserModel"/>
        /// </summary>
        /// <param name="user">The user to update.</param>
        /// <remarks>Remarks: This differs from <see cref="UpdateGuildUserAsync(UserModel)"/> in the sense that
        /// the object is updated externally, and is updated in the database.</remarks>
        public Task UpdateGuildUserAsync(UserModel user);

        /// <summary>
        /// Update a <see cref="GlobalUserModel"/>.
        /// </summary>
        /// <param name="user">The user to update.</param>
        public Task UpdateGlobalUserAsync(GlobalUserModel user);

        /// <summary>
        /// Get a <see cref="UserModel"/> from the database. Unlike <see cref="GetGuildUserAsync"/>, the object is guarunteed to be non-null.
        /// </summary>
        /// <returns>A <see cref="UserModel"/></returns>
        public Task<UserModel> GetOrCreateGuildUserAsync(ulong guildId, ulong userId);

        /// <summary>
        /// Get a <see cref="GlobalUserModel"/> from the database.
        /// </summary>
        /// <param name="userId">The Id of the user to retreive.</param>
        /// <returns>A <see cref="GlobalUserModel"/>, which will have default properties if it did not exist in the databsae.</returns>
        public Task<GlobalUserModel> GetOrCreateGlobalUserAsync(ulong userId);
        
        /// <summary>
        /// Remove a user from the database. A user with infractions will not be removed from the database.
        /// </summary>
        /// <param name="user">The user to attempt to remove.</param>
        /// <returns></returns>
        public Task RemoveUserAsync(UserModel user);

        /// <summary>
        /// Get all users that match a predicate.
        /// </summary>
        /// <param name="predicate">The expression to apply to all users</param>
        /// <returns>A collection of users that match the supplied predicate.</returns>
        public Task<IEnumerable<UserModel>> GetAllUsersAsync(Expression<Func<UserModel, bool>> predicate);
    }
}