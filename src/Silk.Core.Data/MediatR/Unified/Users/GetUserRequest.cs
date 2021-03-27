using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Users
{
    /// <summary>
    /// Request to get a user from the database, or null, if it does not exist.
    /// </summary>
    public record GetUserRequest(ulong GuildId, ulong UserId) : IRequest<User?>;

    /// <summary>
    /// The default handler associated with <see cref="GetUserRequest"/>.
    /// </summary>
    public class GetUserHandler : IRequestHandler<GetUserRequest, User?>
    {
        private readonly GuildContext _db;
        public GetUserHandler(GuildContext db) => _db = db;

        public async Task<User?> Handle(GetUserRequest request, CancellationToken cancellationToken)
        {
            User? user = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId && u.GuildId == request.GuildId, cancellationToken);
            return user;
        }
    }
}