using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Users
{
    /// <summary>
    /// Updates a user in the database.
    /// </summary>
    public record UserUpdateRequest(ulong GuildId, ulong UserId, UserFlag? Flags = null) : IRequest<User>;

    /// <summary>
    /// The default handler for <see cref="UserUpdateRequest"/>.
    /// </summary>
    public class UserUpdateHandler : IRequestHandler<UserUpdateRequest, User>
    {
        private readonly GuildContext _db;

        public UserUpdateHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<User> Handle(UserUpdateRequest request, CancellationToken cancellationToken)
        {
            User user = await _db.Users
                .FirstAsync(u => u.Id == request.UserId && u.GuildId == request.GuildId, cancellationToken);

            user.Flags = request.Flags ?? user.Flags;
            await _db.SaveChangesAsync(cancellationToken);

            return user;
        }
    }
}