using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Users
{
    /// <summary>
    /// Updates users in the database en masse.
    /// </summary>
    public record UserBulkUpdateRequest(IEnumerable<User> Users) : IRequest<IEnumerable<User>>;

    /// <summary>
    /// The default handler for <see cref="UserBulkUpdateRequest"/>.
    /// </summary>
    public class UserBulkUpdateHandler : IRequestHandler<UserBulkUpdateRequest, IEnumerable<User>>
    {
        private readonly GuildContext _db;

        public UserBulkUpdateHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<User>> Handle(UserBulkUpdateRequest request, CancellationToken cancellationToken)
        {
            _db.AttachRange(request.Users);
            await _db.SaveChangesAsync(cancellationToken);
            return request.Users;
        }
    }
}