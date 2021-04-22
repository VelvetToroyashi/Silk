using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Users
{
    /// <summary>
    ///     Request for adding users to the database en masse.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This is not intended to be used with multi-guild updates as
    ///         upon failure, the first element of the user collection is picked, and that Guild Id
    ///         is used to query the users that are already inserted into the database to refine insertion
    ///         queries. Validation should be done outside to ensure no duplicate users exist, as a slow
    ///         branch will be taken if bulk-inserting fails.
    ///     </para>
    /// </remarks>
    public record BulkAddUserRequest(IEnumerable<User> Users) : IRequest<IEnumerable<User>>;

    /// <summary>
    ///     The default handler for <see cref="BulkAddUserRequest" />.
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
            try
            {
                _db.AddRange(request.Users);
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                _db.ChangeTracker.Clear();

                var nonAddedUsers = await _db.Users.Where(u => u.GuildId == request.Users.First().Id).ToListAsync(cancellationToken);
                nonAddedUsers = request.Users.Except(nonAddedUsers).ToList();

                _db.Users.AddRange(nonAddedUsers);


                try
                {
                    await _db.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException) { await AttemptAddUsersSlowAsync(request.Users); }
                // Y'know. I hope whoever forces the catch branch to run has a warm pillow. //
            }
            return request.Users;
        }

        /// <summary>
        ///     Adds users individually to mitigate an entire query failing when adding in bulk.
        ///     <para>This is considerably slower than <see cref="Handle" />.</para>
        /// </summary>
        /// <param name="users">The collection of users to add.</param>
        private async Task AttemptAddUsersSlowAsync(IEnumerable<User> users)
        {
            foreach (var user in users)
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