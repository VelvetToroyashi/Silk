using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using MediatR;
using Microsoft.Extensions.Logging;
using Silk.Data.MediatR;
using Silk.Data.Models;
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
                User? user = await _mediator.Send(new UserRequest.Get { UserId = e.Member.Id, GuildId = e.Guild.Id });
                if (user is null) return;
                user.Flags.Remove(UserFlag.Staff);
                await _mediator.Send(new UserRequest.Update {UserId = user.Id, GuildId = user.GuildId, Flags = user.Flags});
                _logger.LogDebug($"Removed staff role from {e.Member.Id}");
            });
        }
    }
}