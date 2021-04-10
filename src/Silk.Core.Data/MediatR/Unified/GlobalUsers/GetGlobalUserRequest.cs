using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.GlobalUsers
{
    /// <summary>
    ///     Request to get a <see cref="GlobalUser" />.
    /// </summary>
    public record GetGlobalUserRequest(ulong UserId) : IRequest<GlobalUser>;

    /// <summary>
    ///     The default handler for <see cref="GetGlobalUserRequest" />.
    /// </summary>
    public class GetGlobalUserHandler : IRequestHandler<GetGlobalUserRequest, GlobalUser>
    {
        private readonly GuildContext _db;

        public GetGlobalUserHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<GlobalUser> Handle(GetGlobalUserRequest request, CancellationToken cancellationToken)
        {
            GlobalUser? user = await _db.GlobalUsers.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            return user;
        }
    }
}