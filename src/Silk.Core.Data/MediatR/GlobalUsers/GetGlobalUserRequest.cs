using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.GlobalUsers
{
	/// <summary>
	///     Request to get a <see cref="GlobalUserEntity" />.
	/// </summary>
	public record GetGlobalUserRequest(ulong UserId) : IRequest<GlobalUserEntity>;

	/// <summary>
	///     The default handler for <see cref="GetGlobalUserRequest" />.
	/// </summary>
	public class GetGlobalUserHandler : IRequestHandler<GetGlobalUserRequest, GlobalUserEntity>
    {
        private readonly GuildContext _db;

        public GetGlobalUserHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<GlobalUserEntity> Handle(GetGlobalUserRequest request, CancellationToken cancellationToken)
        {
            GlobalUserEntity? user = await _db.GlobalUsers.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            return user;
        }
    }
}