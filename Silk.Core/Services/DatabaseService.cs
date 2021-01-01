using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;
using Silk.Core.Database;
using Silk.Core.Database.Models;

namespace Silk.Core.Services
{
    /// <summary>
    ///     A service class that provides a bridge between <see cref="SilkDbContext" /> and Model classes such as <see cref="UserModel" /> &
    ///     <see cref="GuildModel" />
    ///     This class also provides methods for updating models, and handles Database operations, abstracting it from command classes.
    /// </summary>
    public class DatabaseService
    {
        #region Service ctor

        // This is the only instance of an IDbContextFactory<T> we should need. //
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;
        private ILogger<DatabaseService> _logger;

        public DatabaseService(IDbContextFactory<SilkDbContext> dbFactory, ILogger<DatabaseService> logger) => (_dbFactory, _logger) = (dbFactory, logger);


        #endregion

        
        #region Public Guild-Retrieval methods

        public Task<GuildModel> GetGuildAsync(ulong guildId) =>
            this.GetContext()
                .Guilds
                .Include(g => g.Users)
                .FirstOrDefaultAsync(g => g.Id == guildId);

        public Task<GuildConfigModel> GetConfigAsync(ulong configId) =>
            this.GetContext()
                .GuildConfigs
                .Include(c => c.AllowedInvites)
                .FirstAsync(g => g.GuildId == configId);

        #endregion
        
        
        #region Public User-Retrieval methods

        public async Task<UserModel?> GetGuildUserAsync(ulong guildId, ulong userId)
        {
            SilkDbContext db = this.GetContext();

            GuildModel guild = await db.Guilds.Include(g => g.Users).FirstOrDefaultAsync(g => g.Id == guildId);
            UserModel? user = guild.Users.FirstOrDefault(u => u.Id == userId);

            user ??= new UserModel {Id = userId};
            guild.Users.Add(user);
            await db.SaveChangesAsync();

            return user;
        }


        /// <summary>
        ///     Update a <see cref="UserModel" />.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task UpdateGuildUserAsync(UserModel user, Action<UserModel> updateAction)
        {
            SilkDbContext db = GetContext();
            db.Attach(user);
            updateAction(user);
            await db.SaveChangesAsync();
        }

        public async Task<UserModel> GetOrAddUserAsync(ulong guildId, ulong userId)
        {
            SilkDbContext db = _dbFactory.CreateDbContext();
            GuildModel guild = await db.Guilds.Include(g => g.Users).FirstOrDefaultAsync(g => g.Id == guildId);
            UserModel? user = guild.Users.FirstOrDefault(u => u.Id == userId);
            
            user ??= new UserModel {Id = userId};
            guild.Users.Add(user);
            
            await db.SaveChangesAsync();
            return user;
        }

        
        /// <summary>
        /// Remove a user from the database. A user with infractions will not be removed from the database.
        /// </summary>
        /// <param name="user">The user to attempt to remove.</param>
        /// <returns></returns>
        public async Task RemoveUserAsync(UserModel user)
        {
            SilkDbContext db = _dbFactory.CreateDbContext();
            db.Attach(user);

            if (user.Infractions.Any())
            {
                
            }
            user.Guild.Users.Remove(user);

            await db.SaveChangesAsync();
        }
        #endregion

        #region Internal helper methods

        public SilkDbContext GetContext() => _dbFactory.CreateDbContext();

        #endregion
    }
}