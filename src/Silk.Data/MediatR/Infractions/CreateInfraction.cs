using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;
using Silk.Data.MediatR.Users;

namespace Silk.Data.MediatR.Infractions;

public static class CreateInfraction
{
    public sealed record Request
        (
            Snowflake       GuildID,
            Snowflake       TargetID,
            Snowflake       EnforcerID,
            string          Reason,
            InfractionType  Type,
            DateTimeOffset? Expiration = null
        ) : IRequest<InfractionEntity>;

    internal sealed class Handler : IRequestHandler<Request, InfractionEntity>
    {
        private readonly GuildContext _db;
        private readonly IMediator    _mediator;

        public Handler(GuildContext db, IMediator mediator)
        {
            _db       = db;
            _mediator = mediator;
        }

        public async Task<InfractionEntity> Handle(Request request, CancellationToken cancellationToken)
        {
            int guildInfractionCount = await _db.Infractions
                                                .Where(inf => inf.GuildID == request.GuildID)
                                                .CountAsync(cancellationToken) + 1;

            var infraction = new InfractionEntity
            {
                GuildID    = request.GuildID,
                CaseNumber = guildInfractionCount,
                EnforcerID = request.EnforcerID,
                Reason     = request.Reason,
                ExpiresAt  = request.Expiration,
                CreatedAt  = DateTime.UtcNow,
                TargetID   = request.TargetID,
                Type       = request.Type
            };

            _db.Infractions.Add(infraction);
            await _mediator.Send(new GetOrCreateUser.Request(request.GuildID, request.TargetID), cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);

            return infraction;
        }
    }
}