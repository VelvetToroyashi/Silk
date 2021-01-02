using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Database;
using Silk.Core.Database.Models;

namespace Silk.Core.Services
{
    public interface IDatabaseService
    {
        /// <summary>
        /// Get a <see cref="GuildModel"/> from the database.
        /// </summary>
        /// <returns><see cref="GuildModel"/>with corresponding users.</returns>
        public Task<GuildModel> GetGuildAsync(ulong guildId);
        public Task<GuildConfigModel> GetConfigAsync(ulong configId);
        /// <summary>
        /// Get a <see cref="UserModel"/> from the database.
        /// </summary>
        /// <returns>A <see cref="UserModel?"/>, or null if the user doesn't exist.</returns>
        public Task<UserModel?> GetGuildUserAsync(ulong guildId, ulong userId);

        /// <summary>
        ///     Update a <see cref="UserModel" />.
        /// </summary>
        public Task UpdateGuildUserAsync(UserModel user, Action<UserModel> updateAction);

        
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