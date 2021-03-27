using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using MediatR;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.MediatR.Unified.Users;
using Silk.Core.Data.Models;
using Silk.Extensions;

namespace Silk.Core.EventHandlers
{
    public class RoleRemovedHandler
    {
        private readonly ILogger<RoleRemovedHandler> _logger;
        private readonly IMediator _mediator;
        private const Permissions RequisiteStaffPermissions = Permissions.KickMembers | Permissions.ManageMessages;
        public RoleRemovedHandler(ILogger<RoleRemovedHandler> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task CheckStaffRoles(DiscordClient c, GuildMemberUpdateEventArgs e)
        {
            if (e.RolesBefore.Count <= e.RolesAfter.Count) return;
            if (e.RolesAfter.Any(r => r.Permissions.HasPermission(RequisiteStaffPermissions))) return;
            _ = Task.Run(async () =>
            {
                User? user = await _mediator.Send(new GetUserRequest (e.Member.Id, e.Guild.Id));
                if (user is null) return;

                var flag = user.Flags.HasFlag(UserFlag.EscalatedStaff) ? UserFlag.EscalatedStaff : UserFlag.Staff;
                flag |= UserFlag.InfractionExemption;
                user.Flags = user.Flags.Remove(flag);
                await _mediator.Send(new UpdateUserRequest (user.Id, user.GuildId, user.Flags));
                _logger.LogDebug($"Removed staff role from {e.Member.Id}");
            });
        }
    }
}