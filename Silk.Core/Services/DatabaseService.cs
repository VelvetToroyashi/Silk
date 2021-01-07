using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Silk.Core.Services.Interfaces;
using Silk.Core.Database;
using Silk.Core.Database.Models;

namespace Silk.Core.Services
{

    /// <inheritdoc cref="IDatabaseService"/>
    public class DatabaseService : IDatabaseService
    {
        #region Service ctor

        // This is the only instance of an IDbContextFactory<T> we should need. //
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;
        private ILogger<DatabaseService> _logger;

        public DatabaseService(IDbContextFactory<SilkDbContext> dbFactory, ILogger<DatabaseService> logger) => (_dbFactory, _logger) = (dbFactory, logger);


        #endregion

        
        #region Public Guild-Retrieval methods

        public Task<GuildModel?> GetGuildAsync(ulong guildId)
        {
            SilkDbContext db = this.GetContext();
            var guildQuery = db.Guilds.Include(g => g.Users);
            return guildQuery.FirstAsync(g => g.Id == guildId)!;
        }

        public Task UpdateGuildAsync(GuildModel guild)
        {
            SilkDbContext db = this.GetContext();
            EntityEntry<GuildModel>? entity = db.Attach(guild);
            entity.State = EntityState.Modified;
            return db.SaveChangesAsync();
        }

        public async Task<GuildModel> GetOrCreateGuildAsync(ulong guildId)
        {
            SilkDbContext db = this.GetContext();
            GuildModel? guild = await this.GetGuildAsync(guildId);
            if (guild is null)
            {
                guild = new() { Id = guildId, Users = new(),Prefix = Bot.DefaultCommandPrefix, Configuration = new() };
                db.Guilds.Add(guild);
                await db.SaveChangesAsync();
            }
            return guild;
        }

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
            /*
             * The reason we have to actually assign the attatching to a variable is because this is
             * the only way we can externally get an EntityEntry<T> as far as I'm aware. EFCore holds one internally,
             * but DbContext#Attatch() sets the entity to be unmodified by default. 
             */ 
            EntityEntry<UserModel> userEntity = db.Attach(user);
            userEntity.State = EntityState.Modified;
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