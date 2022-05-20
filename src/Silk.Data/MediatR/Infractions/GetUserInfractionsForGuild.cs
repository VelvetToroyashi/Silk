using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Infractions;

public static class GetUserInfractionsForGuild
{
    public sealed record Request(Snowflake GuildID, Snowflake TargetID) : IRequest<IEnumerable<InfractionEntity>>;

    internal sealed class Handler : IRequestHandler<Request, IEnumerable<InfractionEntity>>
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public async Task<IEnumerable<InfractionEntity>> Handle(Request request, CancellationToken cancellationToken)
        {
            var query = _db.Infractions
                           .Where(inf => inf.TargetID == request.TargetID)
                           .Where(inf => inf.GuildID == request.GuildID);
            
            return await query.ToListAsync(cancellationToken);
        }
    }
}