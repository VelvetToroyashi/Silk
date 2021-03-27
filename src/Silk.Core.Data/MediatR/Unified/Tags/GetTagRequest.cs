using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Tags
{
    /// <summary>
    /// Request to get a <see cref="Tag"/>, or null if it doesn't exist.
    /// </summary>
    public record GetTagRequest(string Name, ulong GuildId) : IRequest<Tag?>;

    /// <summary>
    /// The default handler for <see cref="GetTagRequest"/>.
    /// </summary>
    public class GetTagHandler : IRequestHandler<GetTagRequest, Tag?>
    {
        private readonly GuildContext _db;

        public GetTagHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<Tag?> Handle(GetTagRequest request, CancellationToken cancellationToken)
        {
            Tag? tag = await _db.Tags
                .Include(t => t.OriginalTag)
                .Include(t => t.Aliases)
                .AsSplitQuery()
                .FirstOrDefaultAsync(t =>
                    t.Name.ToLower() == request.Name.ToLower()
                    && t.GuildId == request.GuildId, cancellationToken);
            return tag;
        }
    }
}