using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using MediatR;
using Silk.Core.Constants;
using Silk.Data.MediatR;
using Silk.Data.Models;
using Silk.Extensions;

namespace Silk.Core.EventHandlers
{
    public class RoleAddedHandler
    {
        private readonly IMediator _mediator;
        public RoleAddedHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task CheckStaffRole(DiscordClient c, GuildMemberUpdateEventArgs e)
        {
            if (e.RolesBefore.Count >= e.RolesAfter.Count || e.Member.IsBot) return;
            var isStaff = e.RolesAfter.Except(e.RolesBefore).Any(r => r.HasPermission(PermissionConstants.CacheFlag));
            var isAdmin = e.Member.HasPermission(Permissions.Administrator);
            if (isStaff)
            {
                User? user = await _mediator.Send(new UserRequest.Get (e.Member.Id,  e.Guild.Id));
                var flag = isAdmin ? UserFlag.EscalatedStaff : UserFlag.Staff;
                if (user is not null && !user.Flags.Has(flag))
                {
                    await _mediator.Send(new UserRequest.Update (e.Member.Id, e.Guild.Id) {Flags = user.Flags | flag });
                }
                else
                {
                    await _mediator.Send(new UserRequest.Add (e.Member.Id, e.Guild.Id,  UserFlag.Staff ));
                }
            }
        }
    }
}