using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Tags;

public static class GetTagByGuild
{
    /// <summary>
    /// Request for getting tags for a specific guild, if any.
    /// </summary>
    public sealed record Request(Snowflake GuildID) : IRequest<IEnumerable<TagEntity>>;

    /// <summary>
    /// The default handler for <see cref="Request" />.
    /// </summary>
    internal sealed class Handler : IRequestHandler<Request, IEnumerable<TagEntity>>
    {
        private readonly GuildContext _db;

        public Handler(GuildContext db) => _db = db;

        public async Task<IEnumerable<TagEntity>> Handle(Request request, CancellationToken cancellationToken)
        {
            TagEntity[] tags = await _db
                                    .Tags
                                    .Include(t => t.OriginalTag)
                                    .Include(t => t.Aliases)
                                    .Where(t => t.GuildID == request.GuildID)
                                    .ToArrayAsync(cancellationToken);

            return tags;
        }
    }
}