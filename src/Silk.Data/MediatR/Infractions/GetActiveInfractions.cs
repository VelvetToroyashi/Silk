using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Silk.Data.DTOs.Guilds;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Infractions;

public static class GetActiveInfractions
{
    public sealed record Request(int ShardID, int ShardCount) : IRequest<IEnumerable<Infraction>>;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class Handler : IRequestHandler<Request, IEnumerable<Infraction>>
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;

        public Handler(IDbContextFactory<GuildContext> dbFactory) 
            => _dbFactory = dbFactory;

        public async ValueTask<IEnumerable<Infraction>> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            List<InfractionEntity> infractions = await db.Infractions
                                                         .FromSqlRaw("SELECT * FROM infractions i "    +
                                                                     "WHERE i.expires_at IS NOT NULL " +
                                                                     "AND i.expires_at > NOW() "       +
                                                                     "AND i.processed IS FALSE "        +
                                                                     "AND (i.guild_id::bigint >> 22) % {0} = {1}", request.ShardCount, request.ShardID)
                                                         .ToListAsync(cancellationToken);
            
            return infractions.Select(InfractionEntity.ToDTO)!;
        }
    }
}