using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Users
{
    /// <summary>
    ///     Request for updating a user in the database.
    /// </summary>
    public record UpdateUserRequest(ulong GuildId, ulong UserId, UserFlag? Flags = null) : IRequest<User>;

    /// <summary>
    ///     The default handler for <see cref="UpdateUserRequest" />.
    /// </summary>
    public class UpdateUserHandler : IRequestHandler<UpdateUserRequest, User>
    {
        private readonly GuildContext _db;

        public UpdateUserHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<User> Handle(UpdateUserRequest request, CancellationToken cancellationToken)
        {
            User user = await _db.Users
                .FirstAsync(u => u.Id == request.UserId && u.GuildId == request.GuildId, cancellationToken);

            user.Flags = request.Flags ?? user.Flags;
            await _db.SaveChangesAsync(cancellationToken);

            return user;
        }
    }
}