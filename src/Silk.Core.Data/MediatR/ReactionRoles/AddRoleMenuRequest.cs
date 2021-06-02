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
            (int configId, ulong messageId, var roleDictionary) = request;

            GuildConfig config = await _db.GuildConfigs.FirstOrDefaultAsync(c => c.Id == configId, cancellationToken);
            config.RoleMenus.Add(new() {MessageId = messageId, RoleDictionary = roleDictionary, GuildConfigId = configId});

            await _db.SaveChangesAsync(cancellationToken);

            return default;
        }
    }

}