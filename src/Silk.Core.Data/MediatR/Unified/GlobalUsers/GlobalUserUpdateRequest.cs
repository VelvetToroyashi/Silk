using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.GlobalUsers
{
    /// <summary>
    /// Updates a user who's data is tracked globally.
    /// </summary>
    public record GlobalUserUpdateRequest(ulong UserId) : IRequest<GlobalUser>
    {
        public int Cash { get; init; }
        public DateTime LastCashOut { get; init; }
    }

    /// <summary>
    /// The default handler for <see cref="GlobalUserUpdateRequest"/>.
    /// </summary>
    public class GlobalUserUpdateHandler : IRequestHandler<GlobalUserUpdateRequest, GlobalUser>
    {
        private readonly GuildContext _db;

        public GlobalUserUpdateHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<GlobalUser> Handle(GlobalUserUpdateRequest request, CancellationToken cancellationToken)
        {
            GlobalUser user = await _db.GlobalUsers.FirstAsync(u => u.Id == request.UserId, cancellationToken);
            user.Cash = request.Cash;
            user.LastCashOut = request.LastCashOut;

            await _db.SaveChangesAsync(cancellationToken);
            return user;
        }
    }
}