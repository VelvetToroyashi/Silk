using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Data.DTOs.Guilds;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Infractions;

public static class GetActiveInfractions
{
    public sealed record Request : IRequest<IEnumerable<Infraction>>;

    internal sealed class Handler : IRequestHandler<Request, IEnumerable<Infraction>>
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;
        public Handler(IDbContextFactory<GuildContext> dbFactory) => _dbFactory = dbFactory;

        public async Task<IEnumerable<Infraction>> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            List<InfractionEntity>? infractions = await db.Infractions
                                                           .Where(inf => !inf.Processed)
                                                           .Where(inf => inf.AppliesToTarget)
                                                           .Where(inf => inf.ExpiresAt.HasValue) // This is dangerous because it's not guaranteed to be of a correct type, but eh. //
                                                           .ToListAsync(cancellationToken);

            return infractions.Select(InfractionEntity.ToDTO);
        }
    }
}