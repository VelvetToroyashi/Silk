using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Entities;
using Silk.Core.Data.MediatR.Users;

namespace Silk.Core.Data.MediatR.Infractions
{
    public sealed record CreateInfractionRequest(
        ulong  User,   ulong          Enforcer, ulong     Guild,
        string Reason, InfractionType Type,     DateTime? Expiration,
        bool   HeldAgainstUser = true) : IRequest<InfractionEntity>;

    public class CreateInfractionHandler : IRequestHandler<CreateInfractionRequest, InfractionEntity>
    {
        private readonly GuildContext _db;
        private readonly IMediator    _mediator;

        public CreateInfractionHandler(GuildContext db, IMediator mediator)
        {
            _db = db;
            _mediator = mediator;
        }

        public async Task<InfractionEntity> Handle(CreateInfractionRequest request, CancellationToken cancellationToken)
        {
            int guildInfractionCount = await _db.Infractions
                                                .Where(inf => inf.GuildId == request.Guild)
                                                .CountAsync(cancellationToken) + 1;

            var infraction = new InfractionEntity
            {
                GuildId = request.Guild,
                CaseNumber = guildInfractionCount,
                Enforcer = request.Enforcer,
                Reason = request.Reason,
                HeldAgainstUser = request.HeldAgainstUser,
                Expiration = request.Expiration,
                InfractionTime = DateTime.UtcNow,
                UserId = request.User,
                InfractionType = request.Type
            };

            _db.Infractions.Add(infraction);
            await _mediator.Send(new GetOrCreateUserRequest(request.Guild, request.User), cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);

            return infraction;
        }
    }
}