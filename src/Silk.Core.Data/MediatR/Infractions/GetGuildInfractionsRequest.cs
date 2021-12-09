using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Infractions;

public sealed record GetGuildInfractionsRequest(ulong GuildId) : IRequest<IEnumerable<InfractionEntity>>;

public sealed class GetGuildInfractionHandler : IRequestHandler<GetGuildInfractionsRequest, IEnumerable<InfractionEntity>>
{
    private readonly GuildContext _db;
    public GetGuildInfractionHandler(GuildContext db) => _db = db;

    public async Task<IEnumerable<InfractionEntity>> Handle(GetGuildInfractionsRequest request, CancellationToken cancellationToken)
    {
        List<InfractionEntity>? infractions = await _db
            .Infractions
            .Where(inf => inf.GuildId == request.GuildId)
            .ToListAsync(cancellationToken);

        return infractions;
    }
}