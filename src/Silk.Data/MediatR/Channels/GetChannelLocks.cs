using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Data.Entities.Channels;

namespace Silk.Data.MediatR.Channels;

public static class GetChannelLocks
{
    public record Request : IRequest<IEnumerable<ChannelLockEntity>>;

    internal class Handler : IRequestHandler<Request, IEnumerable<ChannelLockEntity>>
    {
        private readonly GuildContext _db;
        
        public Handler(GuildContext db) => _db = db;

        public async Task<IEnumerable<ChannelLockEntity>> Handle(Request request, CancellationToken cancellationToken)
            => (await _db.ChannelLocks.ToListAsync(cancellationToken)).AsEnumerable();
    }
}