using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.ReactionRoles
{
    public record UpdateReactionRoleRequest(ulong RoleId, ulong ReactionId, int GuildConfigId) : IRequest;

    public class UpdateReactionRoleHandler : IRequestHandler<UpdateReactionRoleRequest>
    {
        private readonly GuildContext _db;
        public UpdateReactionRoleHandler(GuildContext db) => _db = db;

        public async Task<Unit> Handle(UpdateReactionRoleRequest request, CancellationToken cancellationToken)
        {
            GuildConfig config = await _db.GuildConfigs.Include(c => c.ReactionRoles).FirstAsync(g => g.Id == request.GuildConfigId, cancellationToken);

            ReactionRole? role = config.ReactionRoles.FirstOrDefault(r => r.Id == request.RoleId);

            if (role is not null)
            {
                role.EmojiId = request.ReactionId;
                await _db.SaveChangesAsync(cancellationToken);
            }

            return new();
        }
    }
}