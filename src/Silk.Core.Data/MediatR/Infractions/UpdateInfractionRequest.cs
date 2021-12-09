using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Infractions;

public sealed record UpdateInfractionRequest
(
    int                 InfractionId,
    Optional<DateTime?> Expiration   = default,
    Optional<string>    Reason       = default,
    Optional<bool>      Rescinded    = default,
    Optional<bool>      WasEscalated = default
) : IRequest<InfractionEntity>;

public sealed class UpdateInfractionHandler : IRequestHandler<UpdateInfractionRequest, InfractionEntity>
{
    private readonly GuildContext _db;
    public UpdateInfractionHandler(GuildContext db) => _db = db;

    public async Task<InfractionEntity> Handle(UpdateInfractionRequest request, CancellationToken cancellationToken)
    {
        InfractionEntity? infraction = await _db.Infractions
            .FirstAsync(inf => inf.Id == request.InfractionId, cancellationToken);

        if (request.Expiration.HasValue)
            infraction.Expiration = request.Expiration.Value;
            
        if (request.Reason.IsDefined(out var reason))
            infraction.Reason = reason;
            
        if (request.Rescinded.HasValue)
            infraction.HeldAgainstUser = !request.Rescinded.Value;
            
        if (request.WasEscalated.HasValue)
            infraction.EscalatedFromStrike = request.WasEscalated.Value;
            
        await _db.SaveChangesAsync(cancellationToken);
        return infraction;
    }
}