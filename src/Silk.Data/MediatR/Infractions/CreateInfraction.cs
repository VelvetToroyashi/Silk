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
        private readonly IDbContextFactory<GuildContext> _dbFactory;
        private readonly IMediator                       _mediator;

        public Handler(IDbContextFactory<GuildContext> dbFactory, IMediator mediator)
        {
            _dbFactory = dbFactory;
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
            
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            await _mediator.Send(new GetOrCreateUser.Request(request.GuildID, request.TargetID), cancellationToken);
            await _mediator.Send(new GetOrCreateUser.Request(request.GuildID, request.EnforcerID), cancellationToken);
            
            db.Infractions.Add(infraction);
            await db.SaveChangesAsync(cancellationToken);
            
            // This may *not* be necessary? The ID should be set by EF Core. 
            //infraction = await db.Infractions.AsNoTracking().FirstAsync(inf => inf.Id == infraction.Id, cancellationToken); 
            
            return InfractionEntity.ToDTO(infraction);
        }
    }
}