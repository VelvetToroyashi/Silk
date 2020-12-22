#region

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Database;
using Silk.Core.Database.Models;

#endregion

namespace Silk.Core.Services
{
    public class StorageService
    {
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;

        public StorageService(IDbContextFactory<SilkDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<GuildModel> GetGuildAsync(ulong Id)
        {
            using SilkDbContext db = _dbFactory.CreateDbContext();
            return await db.Guilds.FirstOrDefaultAsync(g => g.Id == Id);
        }


        public UserModel GetUserById(GuildModel guild, ulong userId) => 
                guild.Users.FirstOrDefault(u => u.Id == userId);

        public static UserModel GetUser(GuildModel guild, UserModel userId) => 
            guild.Users.FirstOrDefault(u => u.Id == userId.Id);
        
        
    }
}