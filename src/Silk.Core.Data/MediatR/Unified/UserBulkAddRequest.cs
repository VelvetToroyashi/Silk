using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified
{
    public record BulkAddUserRequest(IEnumerable<User> Users) : IRequest<IEnumerable<User>>;

    public class BulkAddUserHandler : IRequestHandler<BulkAddUserRequest, IEnumerable<User>>
    {
        private readonly GuildContext _db;
        public BulkAddUserHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<User>> Handle(BulkAddUserRequest request, CancellationToken cancellationToken)
        {
            _db.AttachRange(request.Users);
            await _db.SaveChangesAsync(cancellationToken);

            return request.Users;
        }
    }
}