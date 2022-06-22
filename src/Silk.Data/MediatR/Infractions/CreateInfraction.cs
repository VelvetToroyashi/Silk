using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.DTOs.Guilds;
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
    ) : IRequest<InfractionDTO>;

    internal sealed class Handler : IRequestHandler<Request, InfractionDTO>
    {
        private readonly GuildContext _db;
        private readonly IMediator    _mediator;

        public Handler(GuildContext db, IMediator mediator)
        {
            _db       = db;
            _mediator = mediator;
        }

        public async Task<InfractionDTO> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            
            // We want to make sure that if two infractions are created simultaneously,
            // they're given different IDs. Otherwise when fetching infractions, an exception
            // is thrown becuase the case id is duplicated.
            int guildInfractionCount = await _db.Infractions.CountAsync(i => i.GuildID == request.GuildID, cancellationToken: cancellationToken);

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

            await _mediator.Send(new GetOrCreateUser.Request(request.GuildID, request.TargetID), cancellationToken);
            await _mediator.Send(new GetOrCreateUser.Request(request.GuildID, request.EnforcerID), cancellationToken);
            
            _db.Infractions.Add(infraction);
            await _db.SaveChangesAsync(cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);

            return InfractionEntity.ToDTO(infraction);
        }
    }
}