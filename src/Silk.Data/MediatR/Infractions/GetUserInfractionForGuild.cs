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
        int?           CaseId = null) : IRequest<Infraction?>;

    internal sealed class Handler : IRequestHandler<Request, Infraction?>
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;
        public Handler(IDbContextFactory<GuildContext> dbFactory) => _dbFactory = dbFactory;

        public async Task<Infraction?> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            InfractionEntity? infraction;

            if (request.CaseId is not null)
            {
                infraction = await db.Infractions
                                      .Where(inf => inf.GuildID == request.GuildID && inf.CaseNumber == request.CaseId)
                                      .SingleOrDefaultAsync(cancellationToken);
            }
            else
            {
                infraction = await db.Infractions
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