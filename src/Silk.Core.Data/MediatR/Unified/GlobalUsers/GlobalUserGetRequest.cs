using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.GlobalUsers
{
    /// <summary>
    /// Gets a <see cref="GlobalUser"/>.
    /// </summary>
    public record GlobalUserGetRequest(ulong UserId) : IRequest<GlobalUser>;

    /// <summary>
    /// The default handler for <see cref="GlobalUserGetRequest"/>.
    /// </summary>
    public class GlobalUserGetHandler : IRequestHandler<GlobalUserGetRequest, GlobalUser>
    {
        private readonly GuildContext _db;

        public GlobalUserGetHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<GlobalUser> Handle(GlobalUserGetRequest request, CancellationToken cancellationToken)
        {
            GlobalUser? user = await _db.GlobalUsers.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            return user;
        }
    }
}