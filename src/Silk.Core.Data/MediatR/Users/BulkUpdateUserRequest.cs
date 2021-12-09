using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Users;

/// <summary>
///     Request for updating users in the database en masse.
/// </summary>
public record BulkUpdateUserRequest(IEnumerable<UserEntity> Users) : IRequest<IEnumerable<UserEntity>>;

/// <summary>
///     The default handler for <see cref="BulkUpdateUserRequest" />.
/// </summary>
public class BulkUpdateUserHandler : IRequestHandler<BulkUpdateUserRequest, IEnumerable<UserEntity>>
{
    private readonly GuildContext _db;

    public BulkUpdateUserHandler(GuildContext db) => _db = db;

    public async Task<IEnumerable<UserEntity>> Handle(BulkUpdateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var user in request.Users)
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
                foreach (var user in request.Users)
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