using System;
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
        public Task<GuildModel> GetGuildAsync(ulong guildId);
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
        ///     Update a <see cref="UserModel" />.
        /// </summary>
        public Task UpdateGuildUserAsync(UserModel user, Action<UserModel> updateAction);

        /// <summary>
        /// Update a <see cref="UserModel"/>
        /// </summary>
        /// <param name="user">The user to update.</param>
        /// <remarks>Remarks: This differs from <see cref="UpdateGuildUserAsync(UserModel, Action{UserModel})"/> in the sense that
        /// the object is updated externally, and is updated in the database.</remarks>
        public Task UpdateGuildUserAsync(UserModel user);
        
        /// <summary>
        /// Get a <see cref="UserModel"/> from the database. Unlike <see cref="GetGuildUserAsync"/>, the object is never null
        /// as it's created internally before returning, if it's null.
        /// </summary>
        /// <returns>A <see cref="UserModel"/></returns>
        public Task<UserModel> GetOrAddUserAsync(ulong guildId, ulong userId);

        /// <summary>
        /// Remove a user from the database. A user with infractions will not be removed from the database.
        /// </summary>
        /// <param name="user">The user to attempt to remove.</param>
        /// <returns></returns>
        public Task RemoveUserAsync(UserModel user);

        
    }
}