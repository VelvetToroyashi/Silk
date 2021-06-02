using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.ReactionRoles
{
    public record UpdateRoleMenuRequest(int ConfigId, ulong MessageId, Dictionary<string, ulong> Roles) : IRequest;

    internal class UpdateRoleMenuHandler : IRequestHandler<UpdateRoleMenuRequest>
    {
        private readonly GuildContext _db;
        public UpdateRoleMenuHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<Unit> Handle(UpdateRoleMenuRequest request, CancellationToken ct)
        {
            // InvalidOpExceptions are up to the caller. Oh well. //
            GuildConfig config = await _db.GuildConfigs
                .Include(c => c.RoleMenus)
                .FirstAsync(c => c.Id == request.ConfigId, ct);

            RoleMenu roleMenu = config.RoleMenus.Single(r => r.MessageId == request.MessageId);
            roleMenu.RoleDictionary = request.Roles;

            await _db.SaveChangesAsync(ct);
            return default;
        }
    }
}