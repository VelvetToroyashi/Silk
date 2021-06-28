using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using MediatR;
using Silk.Core.Data.MediatR.Users;
using Silk.Core.Data.Models;
using Silk.Extensions;
using Silk.Shared.Constants;

namespace Silk.Core.EventHandlers
{
    public class RoleAddedHandler
    {
        private readonly IMediator _mediator;
        public RoleAddedHandler(IMediator mediator) => _mediator = mediator;

        public async Task CheckStaffRole(DiscordClient c, GuildMemberUpdateEventArgs e)
        {
            if (e.RolesBefore.Count >= e.RolesAfter.Count || e.Member.IsBot) return;
            bool isStaff = e.RolesAfter.Except(e.RolesBefore).Any(r => r.Permissions.HasPermission(FlagConstants.CacheFlag));
            bool isAdmin = e.Member.HasPermission(Permissions.Administrator);
            if (isStaff)
            {
                User? user = await _mediator.Send(new GetUserRequest(e.Guild.Id, e.Member.Id));
                UserFlag flag = isAdmin ? UserFlag.EscalatedStaff : UserFlag.Staff;
                if (user is not null && !user.Flags.Has(flag))
                {
                    user.Flags |= flag;
                    await _mediator.Send(new UpdateUserRequest(e.Guild.Id, e.Member.Id, user.Flags));
                }
                else
                {
                    await _mediator.Send(new AddUserRequest(e.Guild.Id, e.Member.Id, flag));
                }
            }
        }
    }
}