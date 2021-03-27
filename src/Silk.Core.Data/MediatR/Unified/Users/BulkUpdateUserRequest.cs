using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Users
{
    /// <summary>
    /// Request for updating users in the database en masse.
    /// </summary>
    public record BulkUpdateUserRequest(IEnumerable<User> Users) : IRequest<IEnumerable<User>>;

    /// <summary>
    /// The default handler for <see cref="BulkUpdateUserRequest"/>.
    /// </summary>
    public class BulkUpdateUserHandler : IRequestHandler<BulkUpdateUserRequest, IEnumerable<User>>
    {
        private readonly GuildContext _db;

        public BulkUpdateUserHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<User>> Handle(BulkUpdateUserRequest request, CancellationToken cancellationToken)
        {
            _db.AttachRange(request.Users);
            await _db.SaveChangesAsync(cancellationToken);
            return request.Users;
        }
    }
}