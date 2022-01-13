using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Users;

public static class UpdateUser
{
    /// <summary>
    /// Request for updating a user in the database.
    /// </summary>
    public sealed record Request(Snowflake GuildID, Snowflake UserID, UserFlag? Flags = null) : IRequest<UserEntity>;

    /// <summary>
    /// The default handler for <see cref="Request" />.
    /// </summary>
    internal sealed class Handler : IRequestHandler<Request, UserEntity>
    {
        private readonly GuildContext _db;

        public Handler(GuildContext db) => _db = db;

        public async Task<UserEntity> Handle(Request request, CancellationToken cancellationToken)
        {
            UserEntity user = await _db.Users
                                       .FirstAsync(u => u.ID == request.UserID && u.GuildID == request.GuildID, cancellationToken);

            user.Flags = request.Flags ?? user.Flags;
            await _db.SaveChangesAsync(cancellationToken);

            return user;
        }
    }
}