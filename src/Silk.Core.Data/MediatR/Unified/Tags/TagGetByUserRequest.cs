using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Tags
{
    public record GetTagByUserRequest(ulong GuildId, ulong OwnerId) : IRequest<IEnumerable<Tag>?>;

    public class GetTagByUserHandler : IRequestHandler<GetTagByUserRequest, IEnumerable<Tag>?>
    {
        private readonly GuildContext _db;
        public GetTagByUserHandler(GuildContext db)
        {
            _db = db;
        }
        public async Task<IEnumerable<Tag>?> Handle(GetTagByUserRequest request, CancellationToken cancellationToken)
        {
            Tag[] tags = await _db
                .Tags
                .Include(t => t.OriginalTag)
                .Include(t => t.Aliases)
                .Where(t => t.GuildId == request.GuildId && t.OwnerId == request.OwnerId)
                .ToArrayAsync(cancellationToken);

            return tags.Any() ? tags : null; // Return null over empty list //
        }
    }
}