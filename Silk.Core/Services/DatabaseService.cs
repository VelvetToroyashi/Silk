using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Silk.Core.Services.Interfaces;
using Silk.Core.Database;
using Silk.Core.Database.Models;

namespace Silk.Core.Services
{

    /// <inheritdoc/>
    public class DatabaseService : IDatabaseService
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

        // Attatch to the context and save. Easy as that. //
        public async Task UpdateGuildUserAsync(UserModel user)
        {
            SilkDbContext db = this.GetContext();
            db.Attach(user);
            await db.SaveChangesAsync();
        }

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
        
        public async Task RemoveUserAsync(UserModel user)
        {
            SilkDbContext db = _dbFactory.CreateDbContext();
            db.Attach(user);

            if (user.Infractions.Any()) return; // If they have infractions, don't remove them. //
            user.Guild.Users.Remove(user);

            await db.SaveChangesAsync();
        }
        #endregion

        
        #region Internal helper methods

        private SilkDbContext GetContext() => _dbFactory.CreateDbContext();

        #endregion
    }
}