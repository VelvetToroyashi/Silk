using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.ReactionRoles
{
    public record AddReactionRoleRequest(ulong RoleId, string EmojiName, ulong MessageId, int GuildConfigId) : IRequest;

    public class AddReactionRoleHandler : IRequestHandler<AddReactionRoleRequest>
    {
        private readonly GuildContext _db;
        public AddReactionRoleHandler(GuildContext db) => _db = db;

        public async Task<Unit> Handle(AddReactionRoleRequest request, CancellationToken cancellationToken)
        {
            var role = new ReactionRole {RoleId = request.RoleId, EmojiName = request.EmojiName, MessageId = request.MessageId, GuildConfigId = request.GuildConfigId};
            _db.Add(role);
            await _db.SaveChangesAsync(cancellationToken);
            return new();
        }
    }
}