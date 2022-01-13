using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Tags;

public static class GetTagByUser
{
    /// <summary>
    /// Request to get all <see cref="TagEntity" />'s created by a User in a Guild
    /// </summary>
    /// <param name="GuildID">The Id of the Guild</param>
    /// <param name="OwnerID">The Id of the User</param>
    public sealed record Request(Snowflake GuildID, Snowflake OwnerID) : IRequest<IEnumerable<TagEntity>>;

    /// <summary>
    /// The default handler for <see cref="Request" />.
    /// </summary>
    internal sealed class GetTagByUserHandler : IRequestHandler<Request, IEnumerable<TagEntity>>
    {
        private readonly GuildContext _db;
        public GetTagByUserHandler(GuildContext db) => _db = db;

        public async Task<IEnumerable<TagEntity>> Handle(Request request, CancellationToken cancellationToken)
        {
            TagEntity[] tags = await _db
                                    .Tags
                                    .Include(t => t.OriginalTag)
                                    .Include(t => t.Aliases)
                                    .Where(t => t.GuildID == request.GuildID && t.OwnerID == request.OwnerID)
                                    .ToArrayAsync(cancellationToken);

            return tags;
        }
    }
}