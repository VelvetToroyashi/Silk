using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Infractions
{
    public sealed record UpdateInfractionRequest(
        int     InfractionId,        DateTime? Expiration,
        string? Reason       = null, bool      Rescinded = false,
        bool    WasEscalated = false) : IRequest<InfractionEntity>;

    public sealed class UpdateInfractionHandler : IRequestHandler<UpdateInfractionRequest, InfractionEntity>
    {
        private readonly GuildContext _db;
        public UpdateInfractionHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<InfractionEntity> Handle(UpdateInfractionRequest request, CancellationToken cancellationToken)
        {
            InfractionEntity? infraction = await _db.Infractions
                                                    .FirstAsync(inf => inf.Id == request.InfractionId, cancellationToken);

            infraction.Expiration = request.Expiration;
            infraction.Reason = request.Reason ?? infraction.Reason;
            infraction.HeldAgainstUser = !request.Rescinded;
            infraction.EscalatedFromStrike = request.WasEscalated;

            await _db.SaveChangesAsync(cancellationToken);
            return infraction;
        }
    }
}