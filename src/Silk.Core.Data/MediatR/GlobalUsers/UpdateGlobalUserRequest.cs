using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.GlobalUsers
{
    /// <summary>
    ///     Request for updating a user who's data is tracked globally.
    /// </summary>
    public record UpdateGlobalUserRequest(ulong UserId) : IRequest<GlobalUser>
    {
        public int Cash { get; init; }
        public DateTime LastCashOut { get; init; }
    }

    /// <summary>
    ///     The default handler for <see cref="UpdateGlobalUserRequest" />.
    /// </summary>
    public class UpdateGlobalUserHandler : IRequestHandler<UpdateGlobalUserRequest, GlobalUser>
    {
        private readonly GuildContext _db;

        public UpdateGlobalUserHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<GlobalUser> Handle(UpdateGlobalUserRequest request, CancellationToken cancellationToken)
        {
            GlobalUser user = await _db.GlobalUsers.FirstAsync(u => u.Id == request.UserId, cancellationToken);
            user.Cash = request.Cash;
            user.LastCashOut = request.LastCashOut;

            await _db.SaveChangesAsync(cancellationToken);
            return user;
        }
    }
}