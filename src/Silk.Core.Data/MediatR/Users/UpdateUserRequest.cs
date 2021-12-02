using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Users
{
	/// <summary>
	///     Request for updating a user in the database.
	/// </summary>
	public record UpdateUserRequest(ulong GuildId, ulong UserId, UserFlag? Flags = null) : IRequest<UserEntity>;

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
                                       .FirstAsync(u => u.Id == request.UserId && u.GuildId == request.GuildId, cancellationToken);

            user.Flags = request.Flags ?? user.Flags;
            await _db.SaveChangesAsync(cancellationToken);

            return user;
        }
    }
}