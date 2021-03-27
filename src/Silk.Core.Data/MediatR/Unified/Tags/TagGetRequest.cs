using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Tags
{
    /// <summary>
    /// Gets a <see cref="Tag"/>, or null if it doesn't exist.
    /// </summary>
    public record TagGetRequest(string Name, ulong GuildId) : IRequest<Tag?>;

    /// <summary>
    /// The default handler for <see cref="TagGetRequest"/>.
    /// </summary>
    public class TagGetHandler : IRequestHandler<TagGetRequest, Tag?>
    {
        private readonly GuildContext _db;

        public TagGetHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<Tag?> Handle(TagGetRequest request, CancellationToken cancellationToken)
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