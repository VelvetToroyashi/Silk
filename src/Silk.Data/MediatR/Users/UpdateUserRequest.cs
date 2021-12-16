using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Users;

/// <summary>
///     Request for updating a user in the database.
/// </summary>
public record UpdateUserRequest(Snowflake GuildID, Snowflake UserID, UserFlag? Flags = null) : IRequest<UserEntity>;

/// <summary>
///     The default handler for <see cref="UpdateUserRequest" />.
/// </summary>
public class UpdateUserHandler : IRequestHandler<UpdateUserRequest, UserEntity>
{
    private readonly GuildContext _db;

    public UpdateUserHandler(GuildContext db) => _db = db;

    public async Task<UserEntity> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        UserEntity user = await _db.Users
                                   .FirstAsync(u => u.ID == request.UserID && u.GuildID == request.GuildID, cancellationToken);

        user.Flags = request.Flags ?? user.Flags;
        await _db.SaveChangesAsync(cancellationToken);

        return user;
    }
}