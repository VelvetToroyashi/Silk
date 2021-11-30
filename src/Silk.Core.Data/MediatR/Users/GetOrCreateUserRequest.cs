using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Results;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Users
{
    /// <summary>
    ///     Request to get a user from the database, or creates one if it does not exist.
    /// </summary>
    public record GetOrCreateUserRequest(ulong GuildId, ulong UserId, UserFlag? Flags = null) : IRequest<Result<UserEntity>>;

    /// <summary>
    ///     The default handler for <see cref="GetOrCreateUserRequest" />.
    /// </summary>
    public class GetOrCreateUserHandler : IRequestHandler<GetOrCreateUserRequest, Result<UserEntity>>
    {
        private readonly GuildContext _db;
        public GetOrCreateUserHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<Result<UserEntity>> Handle(GetOrCreateUserRequest request, CancellationToken cancellationToken)
        {
            UserEntity? user = await _db.Users
                                        .Include(u => u.Guild)
                                        .FirstOrDefaultAsync(u => u.Id == request.UserId && u.GuildId == request.GuildId,
                                                             cancellationToken);

            if (user is not null) return user;
            //Guild guild = await _db.Guilds.FirstAsync(g => g.Id == request.GuildId, cancellationToken);
            user = new()
            {
                Id = request.UserId,
                GuildId = request.GuildId,
                Flags = request.Flags ?? UserFlag.None
            };

            _db.Users.Add(user);
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception e)
            {
                return Result<UserEntity>.FromError(new ExceptionError(e));
            }

            return user;
        }
    }
}