using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.ReactionRoles
{
    public record UpdateReactionRoleRequest(ulong MessageId, ulong RoleId, string EmojiName, int GuildConfigId) : IRequest;

    public class UpdateReactionRoleHandler : IRequestHandler<UpdateReactionRoleRequest>
    {
        private readonly GuildContext _db;
        public UpdateReactionRoleHandler(GuildContext db) => _db = db;

        public async Task<Unit> Handle(UpdateReactionRoleRequest request, CancellationToken cancellationToken)
        {
            GuildConfig config = await _db.GuildConfigs.Include(c => c.RoleMenus).FirstAsync(g => g.Id == request.GuildConfigId, cancellationToken);

            RoleMenu? role = config.RoleMenus.FirstOrDefault(r => r.MessageId == request.MessageId);

            if (role is not null)
            {
                if (!role.RoleDictionary.ContainsKey(request.EmojiName))
                {
                    role.RoleDictionary.Remove(request.EmojiName);
                    role.RoleDictionary.Add(request.EmojiName, request.RoleId);
                }
                else
                {
                    role.RoleDictionary[request.EmojiName] = request.RoleId;
                }


                await _db.SaveChangesAsync(cancellationToken);
            }

            return new();
        }
    }
}