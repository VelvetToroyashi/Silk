#region

using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Silk.Core.Database;
using Silk.Core.Database.Models;
using SilkBot.Extensions;

#endregion

namespace Silk.Core.Tools.EventHelpers
{
    public class RoleRemovedHelper
    {
        //TODO: Implement removing Staff flag from members when their role is removed. 

        private readonly IDbContextFactory<SilkDbContext> _dbFactory;
        public RoleRemovedHelper(IDbContextFactory<SilkDbContext> dbFactory) => _dbFactory = dbFactory;
        
        public Task CheckStaffRoles(DiscordClient c, GuildMemberUpdateEventArgs e)
        {
            
            if (e.RolesBefore.Count <= e.RolesAfter.Count) return Task.CompletedTask;
            Log.Logger.Information("no");
            if (e.RolesAfter.Any(r => r.Permissions.HasPermission(Permissions.KickMembers | Permissions.ManageMessages))) return Task.CompletedTask;
            Log.Logger.Information("no x2");
            Task.Run(async () =>
            {
                await using SilkDbContext db = _dbFactory.CreateDbContext();
                GuildModel guild  = await db.Guilds.FirstAsync(g => g.Id == e.Guild.Id);
                UserModel? user = guild.Users.FirstOrDefault(u => u.Id == e.Member.Id);
                if (user is null) return;
                user.Flags.Remove(UserFlag.Staff);
                await db.SaveChangesAsync();
                Log.Logger.Information($"Removed staff role from {e.Member.Username}");
            });
            return Task.CompletedTask;
        }
    }
}