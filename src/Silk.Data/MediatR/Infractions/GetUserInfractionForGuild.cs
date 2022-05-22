using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.DTOs.Guilds;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Infractions;

public static class GetUserInfractionForGuild
{
    public sealed record Request(
        Snowflake      UserID,
        Snowflake      GuildID,
        InfractionType Type,
        int?           CaseId = null) : IRequest<InfractionDTO?>;

    internal sealed class Handler : IRequestHandler<Request, InfractionDTO?>
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public async Task<InfractionDTO?> Handle(Request request, CancellationToken cancellationToken)
        {
            InfractionEntity? infraction;

            if (request.CaseId is not null)
            {
                infraction = await _db.Infractions
                                      .Where(inf => inf.GuildID == request.GuildID && inf.CaseNumber == request.CaseId)
                                      .SingleOrDefaultAsync(cancellationToken);
            }
            else
            {
                infraction = await _db.Infractions
                                      .Where(inf => inf.TargetID == request.UserID)
                                      .Where(inf => inf.GuildID  == request.GuildID)
                                      .Where(inf => inf.Type     == request.Type)
                                      .Where(inf => inf.AppliesToTarget)
                                      .OrderBy(inf => inf.CaseNumber)
                                      .LastOrDefaultAsync(cancellationToken);
            }

            return InfractionEntity.ToDTO(infraction);
        }
    }
}