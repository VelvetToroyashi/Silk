using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Infractions;

public static class UpdateInfraction
{
    public sealed record Request
        (
            int                       InfractionId,
            Optional<DateTimeOffset?> Expiration      = default,
            Optional<string>          Reason          = default,
            Optional<bool>            AppliesToTarget = default,
            Optional<bool>            Processed       = default,
            Optional<bool>            Escalated        = default,
            Optional<bool>            Notified        = default
        ) : IRequest<InfractionEntity>;

    internal sealed class Handler : IRequestHandler<Request, InfractionEntity>
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public async Task<InfractionEntity> Handle(Request request, CancellationToken cancellationToken)
        {
            var infraction = await _db.Infractions
                                      .FirstAsync(inf => inf.Id == request.InfractionId, cancellationToken);

            if (request.Expiration.HasValue)
                infraction.ExpiresAt = request.Expiration.Value;

            if (request.Reason.IsDefined(out string? reason))
                infraction.Reason = reason;

            if (request.AppliesToTarget.HasValue)
                infraction.AppliesToTarget = !request.AppliesToTarget.Value;
        
            if (request.Processed.HasValue)
                infraction.Processed = request.Processed.Value;

            if (request.Escalated.HasValue)
                infraction.Escalated = request.Escalated.Value;

            if (request.Notified.HasValue)
                infraction.UserNotified = request.Notified.Value;

            await _db.SaveChangesAsync(cancellationToken);
            return infraction;
        }
    }
}