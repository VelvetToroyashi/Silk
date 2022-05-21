using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Users;

public static class GetUser
{
    /// <summary>
    /// Request to get a user from the database, or null, if it does not exist.
    /// </summary>
    public sealed record Request(Snowflake GuildID, Snowflake UserID) : IRequest<UserEntity?>;

    /// <summary>
    /// The default handler associated with <see cref="Request" />.
    /// </summary>
    internal sealed class Handler : IRequestHandler<Request, UserEntity?>
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public async Task<UserEntity?> Handle(Request request, CancellationToken cancellationToken)
        {
            UserEntity? user = await _db.Users
                                        .Include(u => u.History)
                                        .Include(u => u.Infractions)
                                        .FirstOrDefaultAsync(u => u.ID == request.UserID, cancellationToken);
            return user;
        }
    }
}