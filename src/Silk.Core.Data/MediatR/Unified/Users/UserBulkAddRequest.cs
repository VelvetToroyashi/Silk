using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Users
{
    /// <summary>
    /// Adds users to the database en masse.
    /// </summary>
    public record UserBulkAddRequest(IEnumerable<User> Users) : IRequest<IEnumerable<User>>;

    /// <summary>
    /// The default handler for <see cref="UserBulkAddRequest"/>.
    /// </summary>
    public class UserBulkAddHandler : IRequestHandler<UserBulkAddRequest, IEnumerable<User>>
    {
        private readonly GuildContext _db;

        public UserBulkAddHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<User>> Handle(UserBulkAddRequest request, CancellationToken cancellationToken)
        {
            _db.AttachRange(request.Users);
            await _db.SaveChangesAsync(cancellationToken);

            return request.Users;
        }
    }
}