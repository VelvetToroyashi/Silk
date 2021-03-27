using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Users
{
    /// <summary>
    /// Gets a user from the database, and creates one if it does not exist.
    /// </summary>
    public record UserGetOrCreateRequest(ulong GuildId, ulong UserId, UserFlag? Flags = null) : IRequest<User>;

    public class UserGetOrCreateHandler : IRequestHandler<UserGetOrCreateRequest, User>
    {
        private readonly GuildContext _db;
        public UserGetOrCreateHandler(GuildContext db) => _db = db;

        public async Task<User> Handle(UserGetOrCreateRequest request, CancellationToken cancellationToken)
        {
            User? user = await _db.Users
                .Include(u => u.Guild)
                .FirstOrDefaultAsync(u => u.Id == request.UserId && u.GuildId == request.GuildId,
                    cancellationToken);

            if ((User?) user is null)
            {
                //Guild guild = await _db.Guilds.FirstAsync(g => g.Id == request.GuildId, cancellationToken);
                user = new()
                {
                    GuildId = request.GuildId,
                    Id = request.UserId,
                    Flags = request.Flags ?? UserFlag.None,
                };
                //guild.Users.Add(user);
                await _db.SaveChangesAsync(cancellationToken);
            }

            return user;
        }
    }
}