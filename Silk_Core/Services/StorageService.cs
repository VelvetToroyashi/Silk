using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SilkBot.Models;

namespace SilkBot.Services
{
    public class StorageService
    {
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;

        public StorageService(IDbContextFactory<SilkDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public GuildModel GetGuild(ulong Id)
        {
            using SilkDbContext db = _dbFactory.CreateDbContext();
            return db.Guilds.FirstOrDefault(g => g.Id == Id);
        }


        public static UserModel GetUserById(GuildModel guild, ulong userId) => guild.Users.FirstOrDefault(u => u.Id == userId);
        public UserModel GetUserById(Func<GuildModel, bool> guild, ulong userId)
        {
            using SilkDbContext db = _dbFactory.CreateDbContext();
            return db.Guilds.FirstOrDefault(guild).Users.FirstOrDefault(u => u.Id == userId);
        }


        public static UserModel GetUser(GuildModel guild, UserModel userId) => guild.Users.FirstOrDefault(u => u.Id == userId.Id);
        public UserModel GetUser(Func<GuildModel, bool> guild, Func<UserModel, bool> user)
        {
            using SilkDbContext db = _dbFactory.CreateDbContext();
            return db.Guilds.FirstOrDefault(guild).Users.FirstOrDefault(user);
        }


    }
}
