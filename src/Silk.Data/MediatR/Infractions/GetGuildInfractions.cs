using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.DTOs.Guilds;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Infractions;

public static class GetGuildInfractions
{
    public sealed record Request(Snowflake GuildID) : IRequest<IEnumerable<Infraction>>;

    internal sealed class Handler : IRequestHandler<Request, IEnumerable<Infraction>>
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;
        public Handler(IDbContextFactory<GuildContext> dbFactory) => _dbFactory = dbFactory;

        public async Task<IEnumerable<Infraction>> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            List<InfractionEntity>? infractions = await db
                                                       .Infractions
                                                       .Where(inf => inf.GuildID == request.GuildID)
                                                       .ToListAsync(cancellationToken);

            return infractions.Select(InfractionEntity.ToDTO);
        }
    }
}