using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Tags
{
    /// <summary>
    /// Gets tags for a specific guild, if any.
    /// </summary>
    public record TagGetByGuildRequest(ulong GuildId) : IRequest<IEnumerable<Tag>?>;

    /// <summary>
    /// The default handler for <see cref="TagGetByGuildRequest"/>.
    /// </summary>
    public class TagGetByGuildHandler : IRequestHandler<TagGetByGuildRequest, IEnumerable<Tag>?>
    {
        private readonly GuildContext _db;

        public TagGetByGuildHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Tag>?> Handle(TagGetByGuildRequest request, CancellationToken cancellationToken)
        {
            Tag[] tags = await _db
                    .Tags
                    .Include(t => t.OriginalTag)
                    .Include(t => t.Aliases)
                    .Where(t => t.GuildId == request.GuildId)
                    .ToArrayAsync(cancellationToken);

            return tags.Any() ? tags : null;
        }
    }
}