using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Infractions;

public sealed record GetGuildInfractionsRequest(Snowflake GuildID) : IRequest<IEnumerable<InfractionEntity>>;

public sealed class GetGuildInfractionHandler : IRequestHandler<GetGuildInfractionsRequest, IEnumerable<InfractionEntity>>
{
    private readonly GuildContext _db;
    public GetGuildInfractionHandler(GuildContext db) => _db = db;

    public async Task<IEnumerable<InfractionEntity>> Handle(GetGuildInfractionsRequest request, CancellationToken cancellationToken)
    {
        List<InfractionEntity>? infractions = await _db
                                                   .Infractions
                                                   .Where(inf => inf.GuildID == request.GuildID)
                                                   .ToListAsync(cancellationToken);

        return infractions;
    }
}