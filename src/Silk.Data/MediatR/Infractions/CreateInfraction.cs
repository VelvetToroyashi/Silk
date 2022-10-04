using System;
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
    ) : IRequest<Infraction>;

    internal sealed class Handler : IRequestHandler<Request, Infraction>
    {
        private readonly GuildContext _db;
        private readonly IMediator                       _mediator;

        public Handler(GuildContext db, IMediator mediator)
        {
            _db = db;
            _mediator  = mediator;
        }

        public async Task<Infraction> Handle(Request request, CancellationToken cancellationToken)
        {
            var infraction = new InfractionEntity
            {
                GuildID    = request.GuildID,
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
            
            // We have to re-request in order to get the ID.
            infraction = await _db.Infractions.FirstAsync(inf => inf.ID == infraction.ID, cancellationToken); 
            
            return InfractionEntity.ToDTO(infraction);
        }
    }
}