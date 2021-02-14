using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Silk.Core.Database.Models;
using Silk.Core.Services.Interfaces;
using Silk.Extensions;

namespace Silk.Core.EventHandlers
{
    public class RoleRemovedHandler
    {
        private readonly ILogger<RoleRemovedHandler> _logger;
        private readonly IDatabaseService _dbService;
        private const Permissions RequisiteStaffPermissions = Permissions.KickMembers | Permissions.ManageMessages;
        public RoleRemovedHandler(IDatabaseService dbService, ILogger<RoleRemovedHandler> logger)
        {
            _dbService = dbService;
            _logger = logger;
        }

        public async Task CheckStaffRoles(DiscordClient c, GuildMemberUpdateEventArgs e)
        {
            if (e.RolesBefore.Count <= e.RolesAfter.Count) return;
            if (e.RolesAfter.Any(r => r.Permissions.HasPermission(RequisiteStaffPermissions))) return;
            _ = Task.Run(async () =>
            {
                User? user = await _dbService.GetGuildUserAsync(e.Guild.Id, e.Member.Id);
                if (user is null) return;
                user.Flags.Remove(UserFlag.Staff);
                await _dbService.UpdateGuildUserAsync(user);
                _logger.LogDebug($"Removed staff role from {e.Member.Id}");
            });
        }
    }
}