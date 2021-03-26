using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.GlobalUsers
{
    /// <summary>
    /// Updates a user whom's data is tracked globally.
    /// </summary>
    public record UpdateGlobalUserRequest(ulong UserId) : IRequest<GlobalUser>
    {
        public int Cash { get; init; }
        public DateTime LastCashOut { get; init; }
    }

    /// <summary>
    /// The default handler for <see cref="UpdateGlobalUserRequest"/>.
    /// </summary>
    public class GlobalUserUpdateRequest : IRequestHandler<UpdateGlobalUserRequest, GlobalUser>
    {
        private readonly GuildContext _db;
        public GlobalUserUpdateRequest(GuildContext db)
        {
            _db = db;
        }
        public async Task<GlobalUser> Handle(UpdateGlobalUserRequest request, CancellationToken cancellationToken)
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