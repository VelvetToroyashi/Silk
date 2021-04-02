using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using MediatR;
using Silk.Core.Data.MediatR.Unified.Users;
using Silk.Core.Data.Models;
using Silk.Core.Discord.Constants;
using Silk.Extensions;

namespace Silk.Core.Discord.EventHandlers
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
            var isStaff = e.RolesAfter.Except(e.RolesBefore).Any(r => r.HasPermission(FlagConstants.CacheFlag));
            var isAdmin = e.Member.HasPermission(Permissions.Administrator);
            if (isStaff)
            {
                User? user = await _mediator.Send(new GetUserRequest(e.Member.Id, e.Guild.Id));
                var flag = isAdmin ? UserFlag.EscalatedStaff : UserFlag.Staff;
                if (user is not null && !user.Flags.Has(flag))
                {
                    user.Flags |= flag;
                    await _mediator.Send(new UpdateUserRequest(e.Member.Id, e.Guild.Id, user.Flags));
                }
                else
                {
                    await _mediator.Send(new AddUserRequest(e.Member.Id, e.Guild.Id, flag));
                }
            }
        }
    }
}