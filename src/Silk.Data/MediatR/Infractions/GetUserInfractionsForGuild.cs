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

public static class GetUserInfractionsForGuild
{
    public sealed record Request(Snowflake GuildID, Snowflake TargetID) : IRequest<IEnumerable<InfractionDTO>>;

    internal sealed class Handler : IRequestHandler<Request, IEnumerable<InfractionDTO>>
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public async Task<IEnumerable<InfractionDTO>> Handle(Request request, CancellationToken cancellationToken)
        {
            var query = _db.Infractions
                           .Where(inf => inf.TargetID == request.TargetID)
                           .Where(inf => inf.GuildID == request.GuildID);
            
            return (await query.ToListAsync(cancellationToken)).Select(InfractionEntity.ToDTO)!;
        }
    }
}