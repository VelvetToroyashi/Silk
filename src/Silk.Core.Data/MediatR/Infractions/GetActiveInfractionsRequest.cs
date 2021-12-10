using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Infractions;

public record GetActiveInfractionsRequest : IRequest<IEnumerable<InfractionEntity>>;

public class GetCurrentInfractionsHandler : IRequestHandler<GetActiveInfractionsRequest, IEnumerable<InfractionEntity>>
{
    private readonly GuildContext _db;
    public GetCurrentInfractionsHandler(GuildContext db) => _db = db;

    public async Task<IEnumerable<InfractionEntity>> Handle(GetActiveInfractionsRequest request, CancellationToken cancellationToken)
    {
        List<InfractionEntity>? infractions = await _db.Infractions
                                                       .Where(inf => !inf.Processed)
                                                       .Where(inf => inf.AppliesToTarget)
                                                       .Where(inf => inf.ExpiresAt.HasValue) // This is dangerous because it's not guaranteed to be of a correct type, but eh. //
                                                       .ToListAsync(cancellationToken);

        return infractions;
    }
}