using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Silk.Core.Database;
using Silk.Core.Database.Models;
using Silk.Extensions;

namespace Silk.Core.Tools.EventHelpers
{
    public class RoleAddedHandler
    {
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;
        public RoleAddedHandler(IDbContextFactory<SilkDbContext> dbFactory) => _dbFactory = dbFactory;

        public async Task CheckStaffRole(DiscordClient c, GuildMemberUpdateEventArgs e)
        {
            if (e.RolesBefore.Count >= e.RolesAfter.Count) return;
            _ = Task.Run(async () =>
            {
                SilkDbContext db = _dbFactory.CreateDbContext();
                Guild guild = await db.Guilds.Include(g => g.Users).FirstAsync(g => g.Id == e.Guild.Id);
                if (e.RolesAfter.Any(r => r.HasPermission(Permissions.KickMembers | Permissions.ManageMessages)))
                {
                    // I was really stupid to make the oversight of picking the first user in the Database instead of the first user in the guild. ~Velvet. //
                    User? user = guild.Users.FirstOrDefault(u => u.Id == e.Member.Id);
                    if (user is not null)
                    {
                        user.Flags.Add(UserFlag.Staff);
                        Log.Logger.Debug("Logged user as staff from role added event.");
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        user = new() {Id = e.Member.Id, Flags = UserFlag.Staff, Guild = guild};
                        guild.Users.Add(user);
                        Log.Logger.Debug("Logged user as staff from role added event.");
                        await db.SaveChangesAsync();
                    }
                }
            });
        }
    }
}