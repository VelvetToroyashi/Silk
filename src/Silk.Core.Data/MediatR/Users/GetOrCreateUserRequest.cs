using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Users
{
    /// <summary>
    ///     Request to get a user from the database, or creates one if it does not exist.
    /// </summary>
    public record GetOrCreateUserRequest(ulong GuildId, ulong UserId, UserFlag? Flags = null) : IRequest<User>;

    /// <summary>
    ///     The default handler for <see cref="GetOrCreateUserRequest" />.
    /// </summary>
    public class GetOrCreateUserHandler : IRequestHandler<GetOrCreateUserRequest, User>
    {
        private readonly GuildContext _db;
        public GetOrCreateUserHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<User> Handle(GetOrCreateUserRequest request, CancellationToken cancellationToken)
        {
            User? user = await _db.Users
                .Include(u => u.Guild)
                .FirstOrDefaultAsync(u => u.Id == request.UserId && u.GuildId == request.GuildId,
                    cancellationToken);

            if (user is not null) return user;
            //Guild guild = await _db.Guilds.FirstAsync(g => g.Id == request.GuildId, cancellationToken);
            user = new()
            {
                GuildId = request.GuildId,
                Id = request.UserId,
                Flags = request.Flags ?? UserFlag.None
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync(cancellationToken);

            return user;
        }
    }
}