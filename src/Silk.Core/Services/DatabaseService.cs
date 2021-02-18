using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Silk.Core.Services.Interfaces;
using Silk.Data;
using Silk.Data.Models;

namespace Silk.Core.Services
{

    /// <inheritdoc cref="IDatabaseService"/>
    public class DatabaseService : IDatabaseService
    {
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;

        public DatabaseService(IDbContextFactory<SilkDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<Guild?> GetGuildAsync(ulong guildId)
        {
            await using SilkDbContext db = GetDbContext();
            IQueryable<Guild>? guildQuery = db.Guilds.Include(g => g.Users).AsSplitQuery();
            return await guildQuery.FirstAsync(g => g.Id == guildId)!;
        }

        public async Task UpdateGuildAsync(Guild guild)
        {
            await using SilkDbContext db = GetDbContext();
            EntityEntry<Guild>? entity = db.Attach(guild);
            entity.State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        public async Task<Guild> GetOrCreateGuildAsync(ulong guildId)
        {
            await using SilkDbContext db = GetDbContext();
            Guild? guild = await db.Guilds
                .Include(g => g.Users)
                .AsSplitQuery()
                .FirstOrDefaultAsync(g => g.Id == guildId);

            if (guild is not null) return guild;

            guild = new()
            {
                Id = guildId,
                Users = new(),
                Prefix = Bot.DefaultCommandPrefix,
                Configuration = new() {GuildId = guildId, GreetingText = ""}
            };
            db.Guilds.Add(guild);
            await db.SaveChangesAsync();

            return guild;
        }

        public async Task<GuildConfig> GetConfigAsync(ulong guildId)
        {
            await using SilkDbContext db = GetDbContext();
            GuildConfig config = await db.GuildConfigs
                .Include(c => c.AllowedInvites)
                .Include(c => c.SelfAssignableRoles)
                //.AsSplitQuery()
                .FirstAsync(g => g.GuildId == guildId);
            return config;
        }

        public async Task UpdateConfigAsync(GuildConfig config)
        {
            await using SilkDbContext db = GetDbContext();
            EntityEntry<GuildConfig>? entity = db.Attach(config);
            entity.State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        public async Task<User?> GetGuildUserAsync(ulong guildId, ulong userId)
        {
            await using SilkDbContext db = GetDbContext();

            Guild guild = await db.Guilds.Include(g => g.Users).AsSplitQuery().FirstOrDefaultAsync(g => g.Id == guildId);
            User? user = guild.Users.FirstOrDefault(u => u.Id == userId);

            return user;
        }
        public async Task<GlobalUser?> GetGlobalUserAsync(ulong userId)
        {
            await using SilkDbContext db = GetDbContext();

            return await db.GlobalUsers.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<GlobalUser> GetOrCreateGlobalUserAsync(ulong userId)
        {
            await using SilkDbContext db = GetDbContext();
            GlobalUser? user = await db.GlobalUsers.FirstOrDefaultAsync(u => u.Id == userId);

            if (user is not null) return user;

            user = new()
            {
                Id = userId,
                Cash = 0,
                LastCashOut = new(2020, 1, 1)
            };
            db.GlobalUsers.Add(user);
            await db.SaveChangesAsync();
            return user;
        }


        // Attach to the context and save. Easy as that. //
        public async Task UpdateGuildUserAsync(User user)
        {
            await using SilkDbContext db = GetDbContext();
            /*
             * The reason we have to actually assign the attaching to a variable is because this is
             * the only way we can externally get an EntityEntry<T> as far as I'm aware. EFCore holds one internally,
             * but DbContext#Attach() sets the entity to be unmodified by default. 
             */
            User dbUser = (await GetGuildUserAsync(user.Guild.Id, user.Id))!;
            dbUser = user;

            await db.SaveChangesAsync();
        }

        public async Task UpdateGlobalUserAsync(GlobalUser user)
        {
            await using SilkDbContext db = GetDbContext();
            GlobalUser dbUser = (await GetGlobalUserAsync(user.Id))!;

            dbUser.Cash = user.Cash;
            dbUser.LastCashOut = user.LastCashOut;

            db.GlobalUsers.Update(dbUser);
            await db.SaveChangesAsync();
        }

        public async Task<User> GetOrCreateGuildUserAsync(ulong guildId, ulong userId)
        {
            await using SilkDbContext db = GetDbContext();
            Guild guild = await db.Guilds
                .Include(g => g.Users)
                .AsSplitQuery()
                .FirstOrDefaultAsync(g => g.Id == guildId);

            User? user = guild.Users.FirstOrDefault(u => u.Id == userId);
            if (user is not null) return user;
            //VALID
            user = CreateUser(guild, userId);
            guild.Users.Add(user);
            await db.SaveChangesAsync();
            return user;
        }


        public async Task RemoveUserAsync(User user)
        {
            await using SilkDbContext db = GetDbContext();
            db.Attach(user);
            if (user.Infractions.Any()) return; // If they have infractions, don't remove them. //
            user.Guild.Users.Remove(user);

            await db.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync(Expression<Func<User, bool>> predicate)
        {

            await using SilkDbContext db = GetDbContext();
            return db.Users.Where(predicate);
        }

        public async Task<IEnumerable<Infraction>> GetActiveInfractionsAsync()
        {
            await using SilkDbContext db = GetDbContext();
            return await db.Infractions
                .AsNoTracking()
                .Where(i =>
                    i.HeldAgainstUser &&
                    i.Expiration > DateTime.Now &&
                    i.InfractionType == InfractionType.Mute ||
                    i.InfractionType == InfractionType.SoftBan ||
                    i.InfractionType == InfractionType.AutoModMute)
                .ToListAsync();
        }


        private SilkDbContext GetDbContext() => _dbFactory.CreateDbContext();
        private static User CreateUser(Guild guild, ulong userId) => new() {Id = userId, Guild = guild};

    }
}