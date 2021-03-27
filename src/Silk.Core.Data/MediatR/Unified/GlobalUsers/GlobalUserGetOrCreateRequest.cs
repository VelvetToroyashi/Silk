using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.GlobalUsers
{
    /// <summary>
    /// Gets a user who's data is stored globally, or creates it if it does not exist.
    /// </summary>
    public record GlobalUserGetOrCreateRequest(ulong UserId) : IRequest<GlobalUser>;

    /// <summary>
    /// The default handler for <see cref="GlobalUserGetOrCreateRequest"/>.
    /// </summary>
    public class GlobalUserGetOrCreateHandler : IRequestHandler<GlobalUserGetOrCreateRequest, GlobalUser>
    {
        private readonly GuildContext _db;

        public GlobalUserGetOrCreateHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<GlobalUser> Handle(GlobalUserGetOrCreateRequest request, CancellationToken cancellationToken)
        {
            GlobalUser? user = await _db.GlobalUsers.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            user ??= new()
            {
                Id = request.UserId,
                LastCashOut = DateTime.MinValue
            };
            await _db.SaveChangesAsync(cancellationToken);
            return user;
        }
    }
}