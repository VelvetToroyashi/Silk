using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Silk.Core.Database;
using Silk.Core.Database.Models;
using Silk.Core.Services.Interfaces;

namespace Silk.Core.Services
{

    /// <inheritdoc cref="IDatabaseService"/>
    public class DatabaseService : IDatabaseService
    {
        #region Service ctor

        // This is the only instance of an IDbContextFactory<T> we should need. //
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(IDbContextFactory<SilkDbContext> dbFactory, ILogger<DatabaseService> logger) => (_dbFactory, _logger) = (dbFactory, logger);

        #endregion


        #region Public Guild-Retrieval methods

        public async Task<GuildModel?> GetGuildAsync(ulong guildId)
        {
            await using SilkDbContext db = GetContext();
            IQueryable<GuildModel>? guildQuery = db.Guilds.Include(g => g.Users).AsSplitQuery();
            return await guildQuery.FirstAsync(g => g.Id == guildId)!;
        }

        public async Task UpdateGuildAsync(GuildModel guild)
        {
            await using SilkDbContext db = GetContext();
            EntityEntry<GuildModel>? entity = db.Attach(guild);
            entity.State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        public async Task<GuildModel> GetOrCreateGuildAsync(ulong guildId)
        {
            await using SilkDbContext db = GetContext();
            GuildModel? guild = await db.Guilds.FirstOrDefaultAsync(g => g.Id == guildId);
            
            if (guild is null)
            {
                guild = new()
                {
                    Id = guildId, 
                    Users = new(), 
                    Prefix = Bot.DefaultCommandPrefix, 
                    Configuration = new() { GuildId = guildId }
                };
                db.Guilds.Add(guild);
                await db.SaveChangesAsync();
            }
            
            return guild;
        }

        public async Task<GuildConfigModel> GetConfigAsync(ulong configId)
        {
            await using SilkDbContext db = GetContext();
            GuildConfigModel config = await db.GuildConfigs
                .Include(c => c.AllowedInvites)
                .Include(c => c.SelfAssignableRoles)
                //.AsSplitQuery()
                .FirstAsync(g => g.GuildId == configId);
            return config;
        }

        #endregion


        #region Public User-Retrieval methods

        public async Task<UserModel?> GetGuildUserAsync(ulong guildId, ulong userId)
        {
            await using SilkDbContext db = GetContext();

            GuildModel guild = await db.Guilds.Include(g => g.Users).AsSplitQuery().FirstOrDefaultAsync(g => g.Id == guildId);
            UserModel? user = guild.Users.FirstOrDefault(u => u.Id == userId);

            return user;
        }
        public async Task<GlobalUserModel?> GetGlobalUserAsync(ulong userId)
        {
            await using SilkDbContext db = GetContext();

            GlobalUserModel? user = await db.GlobalUsers.FirstOrDefaultAsync(u => u.Id == userId);

            return user;
        }
        
        public async Task<GlobalUserModel> GetOrCreateGlobalUserAsync(ulong userId)
        {
            await using SilkDbContext db = GetContext();
            GlobalUserModel? user = await db.GlobalUsers.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null)
            {
                user = new()
                {
                    Id = userId,
                    Cash = 0,
                    Items = new(),
                    LastCashOut = DateTime.MinValue
                };
                await db.SaveChangesAsync();
            }
            return user;
        }
        

        // Attatch to the context and save. Easy as that. //
        public async Task UpdateGuildUserAsync(UserModel user)
        {
            await using SilkDbContext db = GetContext();
            /*
             * The reason we have to actually assign the attatching to a variable is because this is
             * the only way we can externally get an EntityEntry<T> as far as I'm aware. EFCore holds one internally,
             * but DbContext#Attatch() sets the entity to be unmodified by default. 
             */
            EntityEntry<UserModel> userEntity = db.Attach(user);
            userEntity.State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        public async Task UpdateGlobalUserAsync(GlobalUserModel user)
        {
            await using SilkDbContext db = GetContext();
            
            EntityEntry<GlobalUserModel> userEntity = db.Attach(user);
            userEntity.State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        public async Task<UserModel> GetOrCreateGuildUserAsync(ulong guildId, ulong userId)
        {
            await using SilkDbContext db = GetContext();
            GuildModel guild = await db.Guilds
                .Include(g => g.Users)
                .AsSplitQuery()
                .FirstOrDefaultAsync(g => g.Id == guildId);
            
            UserModel? user = guild.Users.FirstOrDefault(u => u.Id == userId);
            if (user is null)
            {
                //VALID
                user = CreateUser(guild, userId);
                guild.Users.Add(user);
                await db.SaveChangesAsync();
            }
            return user;
        }


        public async Task RemoveUserAsync(UserModel user)
        {
            await using SilkDbContext db = GetContext();
            db.Attach(user);
            if (user.Infractions.Any()) return; // If they have infractions, don't remove them. //
            user.Guild.Users.Remove(user);

            await db.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserModel>> GetAllUsersAsync(Expression<Func<UserModel, bool>> predicate)
        {
            await using SilkDbContext db = GetContext();
            return db.Users.Where(predicate);
        }

        #endregion


        #region Internal helper methods

        private SilkDbContext GetContext() => _dbFactory.CreateDbContext();
        private UserModel CreateUser(GuildModel guild, ulong userId) => new() {Id = userId, Guild = guild};

        #endregion
    }
}