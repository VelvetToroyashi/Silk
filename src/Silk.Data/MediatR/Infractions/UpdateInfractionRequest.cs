using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.DTOs.Guilds;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Infractions;

public static class UpdateInfraction
{
    public sealed record Request
    (
        int CaseID,
        Snowflake GuildID,
        Optional<DateTimeOffset?> Expiration      = default,
        Optional<string>          Reason          = default,
        Optional<bool>            AppliesToTarget = default,
        Optional<bool>            Processed       = default,
        Optional<bool>            Escalated       = default,
        Optional<bool>            Notified        = default
    ) : IRequest<Infraction>;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class Handler : IRequestHandler<Request, Infraction>
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;

        public Handler(IDbContextFactory<GuildContext> dbFactory) 
            => _dbFactory = dbFactory;

        public async ValueTask<Infraction> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            var infraction = await db.Infractions
                                     .AsTracking()
                                     .FirstAsync(inf => inf.CaseNumber == request.CaseID && 
                                                        inf.GuildID == request.GuildID, cancellationToken);

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

            await db.SaveChangesAsync(cancellationToken);
            return InfractionEntity.ToDTO(infraction);
        }
    }
}