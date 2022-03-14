using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Users;

public static class BulkUpdateUser
{
    /// <summary>
    /// Request for updating users in the database en masse.
    /// </summary>
    public sealed record Request(IEnumerable<UserEntity> Users) : IRequest<IEnumerable<UserEntity>>;

    /// <summary>
    /// The default handler for <see cref="Request" />.
    /// </summary>
    internal sealed class Handler : IRequestHandler<Request, IEnumerable<UserEntity>>
    {
        private readonly GuildContext _db;

        public Handler(GuildContext db) => _db = db;

        public async Task<IEnumerable<UserEntity>> Handle(Request request, CancellationToken cancellationToken)
        {
            try
            {
                foreach (UserEntity user in request.Users)
                {
                    EntityEntry<UserEntity> state = _db.Attach(user);
                    state.State = EntityState.Modified;
                }

                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException) // Data wasn't actually modified. Save individually. //
            {
                try
                {
                    foreach (UserEntity user in request.Users)
                    {
                        EntityEntry<UserEntity> state = _db.Attach(user);
                        state.State = EntityState.Modified;
                        await _db.SaveChangesAsync(cancellationToken);
                    }
                }
                catch
                {
                    /* Continue. */
                }
            }
            return request.Users;
        }
    }
}