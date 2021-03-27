using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Users
{
    /// <summary>
    /// Gets a user from the database, or null, if it does not exist.
    /// </summary>
    public record UserGetRequest(ulong GuildId, ulong UserId) : IRequest<User?>;

    /// <summary>
    /// The default handler associated with <see cref="UserGetRequest"/>.
    /// </summary>
    public class UserGetHandler : IRequestHandler<UserGetRequest, User?>
    {
        private readonly GuildContext _db;
        public UserGetHandler(GuildContext db) => _db = db;

        public async Task<User?> Handle(UserGetRequest request, CancellationToken cancellationToken)
        {
            User? user = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId && u.GuildId == request.GuildId, cancellationToken);
            return user;
        }
    }
}