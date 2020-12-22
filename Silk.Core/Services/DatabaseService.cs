#region

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Database;
using Silk.Core.Database.Models;

#endregion

namespace Silk.Core.Services
{
    /// <summary>
    /// A service class that provides a bridge between <see cref="SilkDbContext"/> and Model classes such as <see cref="UserModel"/> & <see cref="GuildModel"/>
    ///
    /// This class also provides methods for updating models, and handles Database operations, abstracting it from command classes.
    /// </summary>
    public class DatabaseService
    {
        // This is the only instance of an IDbContextFactory<T> we should need. //
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;

        public DatabaseService(IDbContextFactory<SilkDbContext> dbFactory) => _dbFactory = dbFactory;

        public async Task<GuildModel> GetGuildAsync(ulong guildId)
        {
            await using SilkDbContext db = _dbFactory.CreateDbContext();
            return await db.Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
        }

        public async Task<UserModel?> GetGuildUserAsync(ulong guildId, ulong? userId)
        {
            if (!userId.HasValue) return null;
            
            await using SilkDbContext db = _dbFactory.CreateDbContext();
            
            GuildModel guild = await db.Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
            UserModel? user = guild.Users.FirstOrDefault(u => u.Id == userId);

            user ??= new() {Id = userId.Value};
            await db.SaveChangesAsync();
            
            return user;
        }
        

        /// <summary>
        /// Update a <see cref="UserModel"/>.
        /// </summary>
        /// <param name="userId">The Id of the user to update.</param>
        /// <param name="guildId"></param>
        /// <param name="updateAction"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task UpdateGuildUserAsync(ulong userId, ulong guildId, Action<UserModel> updateAction)
        {
            await using SilkDbContext db = _dbFactory.CreateDbContext();
            UserModel? user = await GetGuildUserAsync(userId, guildId);
            if (user is null) throw new ArgumentNullException($"{nameof(userId)} cannot be null.");
            updateAction(user);
            await db.SaveChangesAsync();
        }


    }
}