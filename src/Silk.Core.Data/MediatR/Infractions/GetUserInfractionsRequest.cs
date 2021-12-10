using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Infractions;

public record GetUserInfractionsRequest(Snowflake GuildID, Snowflake TargetID) : IRequest<IEnumerable<InfractionEntity>>;

public class GetUserInfractionsHandler : IRequestHandler<GetUserInfractionsRequest, IEnumerable<InfractionEntity>>
{
    private readonly GuildContext _db;
    public GetUserInfractionsHandler(GuildContext db) => _db = db;

    public async Task<IEnumerable<InfractionEntity>> Handle(GetUserInfractionsRequest request, CancellationToken cancellationToken)
    {
        UserEntity? user = await _db
                                .Users
                                .Include(u => u.Infractions)
                                .FirstOrDefaultAsync(u => u.ID == request.TargetID && u.GuildID == request.GuildID, cancellationToken);

        return user?.Infractions ?? Array.Empty<InfractionEntity>().AsEnumerable();
    }
}