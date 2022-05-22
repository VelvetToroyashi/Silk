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
    public sealed record Request : IRequest<IEnumerable<InfractionDTO>>;

    internal sealed class Handler : IRequestHandler<Request, IEnumerable<InfractionDTO>>
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public async Task<IEnumerable<InfractionDTO>> Handle(Request request, CancellationToken cancellationToken)
        {
            List<InfractionEntity>? infractions = await _db.Infractions
                                                           .Where(inf => !inf.Processed)
                                                           .Where(inf => inf.AppliesToTarget)
                                                           .Where(inf => inf.ExpiresAt.HasValue) // This is dangerous because it's not guaranteed to be of a correct type, but eh. //
                                                           .ToListAsync(cancellationToken);

            return infractions.Select(InfractionEntity.ToDTO);
        }
    }
}