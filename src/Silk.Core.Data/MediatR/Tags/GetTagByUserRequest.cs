using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Tags;

/// <summary>
///     Request to get all <see cref="TagEntity" />'s created by a User in a Guild
/// </summary>
/// <param name="GuildID">The Id of the Guild</param>
/// <param name="OwnerID">The Id of the User</param>
public record GetTagByUserRequest(Snowflake GuildID, Snowflake OwnerID) : IRequest<IEnumerable<TagEntity>>;

/// <summary>
///     The default handler for <see cref="GetTagByUserRequest" />.
/// </summary>
public class GetTagByUserHandler : IRequestHandler<GetTagByUserRequest, IEnumerable<TagEntity>>
{
    private readonly GuildContext _db;
    public GetTagByUserHandler(GuildContext db) => _db = db;

    public async Task<IEnumerable<TagEntity>> Handle(GetTagByUserRequest request, CancellationToken cancellationToken)
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