using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Guilds;

/// <summary>
///     Request for retrieving a <see cref="GuildEntity" />.
/// </summary>
/// <param name="GuildID">The Id of the Guild</param>
public record GetGuildRequest(Snowflake GuildID) : IRequest<GuildEntity>;

/// <summary>
///     The default handler for <see cref="GetGuildRequest" />.
/// </summary>
public class GetGuildHandler : IRequestHandler<GetGuildRequest, GuildEntity>
{
    private readonly GuildContext _db;
    public GetGuildHandler(GuildContext db) => _db = db;


    public async Task<GuildEntity> Handle(GetGuildRequest request, CancellationToken cancellationToken)
    {
        GuildEntity? guild = await _db.Guilds
                                      .AsSplitQuery()
                                      .AsNoTracking()
                                      .Include(g => g.Users)
                                      .Include(g => g.Infractions)
                                      .Include(g => g.Configuration)
                                      .FirstOrDefaultAsync(g => g.ID == request.GuildID, cancellationToken);

        return guild;
    }
}