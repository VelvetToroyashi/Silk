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
        private readonly SilkDbContext _db;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(SilkDbContext db, ILogger<DatabaseService> logger) => (_db, _logger) = (db, logger);

        #endregion


        #region Public Guild-Retrieval methods

        public async Task<Guild?> GetGuildAsync(ulong guildId)
        {
            IQueryable<Guild>? guildQuery = _db.Guilds.Include(g => g.Users).AsSplitQuery();
            return await guildQuery.FirstAsync(g => g.Id == guildId)!;
        }

        public async Task UpdateGuildAsync(Guild guild)
        {
            EntityEntry<Guild>? entity = _db.Attach(guild);
            entity.State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }

        public async Task<Guild> GetOrCreateGuildAsync(ulong guildId)
        {
            Guild? guild = await _db.Guilds
                .Include(g => g.Users)
                .AsSplitQuery()
                .FirstOrDefaultAsync(g => g.Id == guildId);

            if (guild is not null) return guild;
            
            guild = new()
            {
                Id = guildId, 
                Users = new(), 
                Prefix = Bot.DefaultCommandPrefix, 
                Configuration = new() { GuildId = guildId }
            };
            await _db.Guilds.AddAsync(guild);
            await _db.SaveChangesAsync();

            return guild;
        }

        public async Task<GuildConfig> GetConfigAsync(ulong configId)
        {

            GuildConfig config = await _db.GuildConfigs
                .Include(c => c.AllowedInvites)
                .Include(c => c.SelfAssignableRoles)
                //.AsSplitQuery()
                .FirstAsync(g => g.GuildId == configId);
            return config;
        }

        public async Task UpdateConfigAsync(GuildConfig config)
        {

            EntityEntry<GuildConfig>? entity = _db.Attach(config);
            entity.State = EntityState.Modified;
            await _db.SaveChangesAsync();
        }

        #endregion


        #region Public User-Retrieval methods

        public async Task<User?> GetGuildUserAsync(ulong guildId, ulong userId)
        {
            Guild guild = await _db.Guilds
                .Include(g => g.Users)
                .AsSplitQuery()
                .FirstOrDefaultAsync(g => g.Id == guildId);
            
            User? user = guild.Users.FirstOrDefault(u => u.Id == userId);
            return user;
        }
        public async Task<GlobalUser?> GetGlobalUserAsync(ulong userId) =>  await _db.GlobalUsers.FirstOrDefaultAsync(u => u.Id == userId);
        
        public async Task<GlobalUser> GetOrCreateGlobalUserAsync(ulong userId)
        {
            GlobalUser? user = await _db.GlobalUsers.FirstOrDefaultAsync(u => u.Id == userId);
            
            if (user is not null) return user;
            
            user = new()
            {
                Id = userId,
                Cash = 0,
                Items = new(),
                LastCashOut = new(2020, 1, 1)
            };
            _db.GlobalUsers.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }
        

        // Attach to the context and save. Easy as that. //
        public async Task UpdateGuildUserAsync(User user)
        {
            User dbUser = (await GetGuildUserAsync(user.Guild.Id, user.Id))!;
            // Someone's gonna complain about this. I do not care ~Velvet //
            dbUser = user;

            await _db.SaveChangesAsync();
        }

        public async Task UpdateGlobalUserAsync(GlobalUser user)
        {
            GlobalUser dbUser = (await GetGlobalUserAsync(user.Id))!;
            
            dbUser.Cash = user.Cash;
            dbUser.Items = user.Items;
            dbUser.LastCashOut = user.LastCashOut;

            _db.GlobalUsers.Update(dbUser);
            await _db.SaveChangesAsync();
        }

        public async Task<User> GetOrCreateGuildUserAsync(ulong guildId, ulong userId)
        {
            Guild guild = await _db.Guilds
                .Include(g => g.Users)
                .AsSplitQuery()
                .FirstOrDefaultAsync(g => g.Id == guildId);
            
            User? user = guild.Users.FirstOrDefault(u => u.Id == userId);
            if (user is not null) return user;
            //VALID
            user = CreateUser(guild, userId);
            guild.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }


        public async Task RemoveUserAsync(User user)
        {
            _db.Attach(user);
            if (user.Infractions.Any()) return; // If they have infractions, don't remove them. //
            user.Guild.Users.Remove(user);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>(Expression<Func<T, bool>> predicate) where T : class => _db.Set<T>().Where(predicate);

        public async Task<IEnumerable<Infraction>> GetActiveInfractionsAsync()
        {
            return await _db.Infractions
                .AsNoTracking()
                .Where(i => i.HeldAgainstUser &&
                            i.Expiration > DateTime.Now &&
                            i.InfractionType == InfractionType.Mute ||
                            i.InfractionType == InfractionType.SoftBan ||
                            i.InfractionType == InfractionType.AutoModMute)
                .AsQueryable()
                .ToListAsync();
        }

        #endregion


        #region Internal helper methods

        private static User CreateUser(Guild guild, ulong userId) => new() {Id = userId, Guild = guild};
        
        
        #endregion
    }
}