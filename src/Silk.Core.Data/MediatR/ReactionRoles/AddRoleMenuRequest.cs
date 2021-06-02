using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.ReactionRoles
{
    public record AddRoleMenuRequest(int ConfigId, ulong MessageId, Dictionary<string, ulong> RoleDictionary) : IRequest;

    internal class AddRoleMenuHandler : IRequestHandler<AddRoleMenuRequest>
    {
        private readonly GuildContext _db;
        public AddRoleMenuHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<Unit> Handle(AddRoleMenuRequest request, CancellationToken cancellationToken)
        {
            GuildConfig config = await _db.GuildConfigs.FirstOrDefaultAsync(c => c.Id == request.ConfigId, cancellationToken);
            config.RoleMenus.Add(new() {MessageId = request.MessageId, RoleDictionary = request.RoleDictionary, GuildConfigId = request.ConfigId});

            await _db.SaveChangesAsync(cancellationToken);

            return default;
        }
    }

}