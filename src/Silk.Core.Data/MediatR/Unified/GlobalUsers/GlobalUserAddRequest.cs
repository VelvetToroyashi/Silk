using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.GlobalUsers
{
    /// <summary>
    /// Adds a user who's information is tracked globally rather than per-guild.
    /// </summary>
    public record GlobalUserAddRequest(ulong UserId, int? Cash) : IRequest<GlobalUser>;

    /// <summary>
    /// The default handler for <see cref="GlobalUserAddRequest"/>.
    /// </summary>
    public class GlobalUserAddHandler : IRequestHandler<GlobalUserAddRequest, GlobalUser>
    {
        private readonly GuildContext _db;
        public GlobalUserAddHandler(GuildContext db)
        {
            _db = db;
        }
        public async Task<GlobalUser> Handle(GlobalUserAddRequest request, CancellationToken cancellationToken)
        {
            GlobalUser user = new()
            {
                Id = request.UserId,
                Cash = request.Cash ?? 0,
                LastCashOut = DateTime.MinValue
            };
            _db.GlobalUsers.Add(user);
            await _db.SaveChangesAsync(cancellationToken);
            return user;
        }
    }
}