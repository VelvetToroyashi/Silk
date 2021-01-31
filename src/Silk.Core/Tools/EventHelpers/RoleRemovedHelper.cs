using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Serilog;
using Silk.Core.Database.Models;
using Silk.Core.Services.Interfaces;
using Silk.Extensions;

namespace Silk.Core.Tools.EventHelpers
{
    public class RoleRemovedHelper
    {
        //TODO: Implement removing Staff flag from members when their role is removed. 

        private readonly ILogger<RoleRemovedHelper> _logger;
        private readonly IDatabaseService _dbService;
        public RoleRemovedHelper(IDatabaseService dbService) => _dbService = dbService;

        public Task CheckStaffRoles(DiscordClient c, GuildMemberUpdateEventArgs e)
        {
            if (e.Handled) return Task.CompletedTask;
            if (e.RolesBefore.Count <= e.RolesAfter.Count) return Task.CompletedTask;
            if (e.RolesAfter.Any(r => r.Permissions.HasPermission(Permissions.KickMembers | Permissions.ManageMessages))) return Task.CompletedTask;
            Task.Run(async () =>
            {
                User? user = await _dbService.GetGuildUserAsync(e.Guild.Id, e.Member.Id);
                if (user is null) return;
                user.Flags.Remove(UserFlag.Staff);
                await _dbService.UpdateGuildUserAsync(user);
                Log.Logger.Information($"Removed staff role from {e.Member.Username}");
            });
            return Task.CompletedTask;
        }
    }
}