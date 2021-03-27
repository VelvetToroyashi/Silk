using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Users
{
    /// <summary>
    /// Request for adding users to the database en masse.
    /// </summary>
    public record BulkAddUserRequest(IEnumerable<User> Users) : IRequest<IEnumerable<User>>;

    /// <summary>
    /// The default handler for <see cref="BulkAddUserRequest"/>.
    /// </summary>
    public class BulkAddUserHandler : IRequestHandler<BulkAddUserRequest, IEnumerable<User>>
    {
        private readonly GuildContext _db;

        public BulkAddUserHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<User>> Handle(BulkAddUserRequest request, CancellationToken cancellationToken)
        {
            foreach (var user in request.Users)
            {
                try
                {
                    _db.Add(user);
                    await _db.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException) { }
                finally { _db.ChangeTracker.Clear(); }
            }
            return request.Users;
        }
    }
}