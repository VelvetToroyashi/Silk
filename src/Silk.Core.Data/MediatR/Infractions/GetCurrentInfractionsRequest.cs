using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Infractions
{
    public record GetCurrentInfractionsRequest : IRequest<IEnumerable<InfractionEntity>>;

    public class GetCurrentInfractionsHandler : IRequestHandler<GetCurrentInfractionsRequest, IEnumerable<InfractionEntity>>
    {
        private readonly GuildContext _db;
        public GetCurrentInfractionsHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<InfractionEntity>> Handle(
            GetCurrentInfractionsRequest request,
            CancellationToken            cancellationToken)
        {
            List<InfractionEntity>? infractions = await _db.Infractions
                                                           .Where(inf => !inf.Handled)
                                                           .Where(inf => inf.HeldAgainstUser)
                                                           .Where(inf => inf.Expiration.HasValue) // This is dangerous because it's not guaranteed to be of a correct type, but eh. //
                                                           .ToListAsync(cancellationToken);

            return infractions;
        }
    }
}