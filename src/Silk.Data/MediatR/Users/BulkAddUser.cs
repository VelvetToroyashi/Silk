using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Users;

public static class BulkAddUser
{
    /// <summary>
    /// Request for adding users to the database en masse.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is not intended to be used with multi-guild updates as
    /// upon failure, the first element of the user collection is picked, and that Guild Id
    /// is used to query the users that are already inserted into the database to refine insertion
    /// queries. Validation should be done outside to ensure no duplicate users exist, as a slow
    /// branch will be taken if bulk-inserting fails.
    /// </para>
    /// </remarks>
    public sealed record Request(IEnumerable<UserEntity> Users) : IRequest<IEnumerable<UserEntity>>;

    /// <summary>
    /// The default handler for <see cref="Request" />.
    /// </summary>
    internal sealed class Handler : IRequestHandler<Request, IEnumerable<UserEntity>>
    {
        private readonly GuildContext _db;

        public Handler(GuildContext db) => _db = db;

        public async Task<IEnumerable<UserEntity>> Handle(Request request, CancellationToken  cancellationToken)
        {
            var users = await _db.Users
                                  .Where(u => u.GuildID == request.Users.First().GuildID)
                                  .Select(u => u.ID)
                                  .ToListAsync(cancellationToken);
            
            _db.AddRange(request.Users.ExceptBy(users, u => u.ID));
            
            await _db.SaveChangesAsync(cancellationToken);

            return request.Users;
        }

        /// <summary>
        /// Adds users individually to mitigate an entire query failing when adding in bulk.
        /// <para>This is considerably slower than <see cref="Handle" />.</para>
        /// </summary>
        /// <param name="users">The collection of users to add.</param>
        private async Task AttemptAddUsersSlowAsync(IEnumerable<UserEntity> users)
        {
            foreach (UserEntity user in users)
            {
                try
                {
                    // This is slow and expensive*. //
                    _db.ChangeTracker.Clear(); // Uncertain that SaveChangesAsync clears this. 
                    _db.Users.Add(user); // The issue on github (https://github.com/dotnet/efcore/issues/9118) is still open. //
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    /* I give up. Screw you. */
                }
            }
        }
    }
}