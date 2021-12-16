using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Users;

/// <summary>
///     Request to get a user from the database, or null, if it does not exist.
/// </summary>
public record GetUserRequest(Snowflake GuildID, Snowflake UserID) : IRequest<UserEntity?>;

/// <summary>
///     The default handler associated with <see cref="GetUserRequest" />.
/// </summary>
public class GetUserHandler : IRequestHandler<GetUserRequest, UserEntity?>
{
    private readonly GuildContext _db;
    public GetUserHandler(GuildContext db) => _db = db;

    public async Task<UserEntity?> Handle(GetUserRequest request, CancellationToken cancellationToken)
    {
        UserEntity? user = await _db.Users
                                    .FirstOrDefaultAsync(u => 
                                                             u.ID == request.UserID && 
                                                             u.GuildID == request.GuildID, cancellationToken);
        return user;
    }
}