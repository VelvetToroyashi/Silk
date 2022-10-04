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
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public async Task<IEnumerable<Infraction>> Handle(Request request, CancellationToken cancellationToken)
        {
            
            
            List<InfractionEntity>? infractions = await _db
                                                       .Infractions
                                                       .Where(inf => inf.GuildID == request.GuildID)
                                                       .ToListAsync(cancellationToken);

            return infractions.Select(InfractionEntity.ToDTO);
        }
    }
}