using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Guilds
{
    public record GetGuildRequest(ulong GuildId) : IRequest<Guild>;

    public class GuildGetRequest : IRequestHandler<GetGuildRequest, Guild>
    {
        private readonly GuildContext _db;
        public GuildGetRequest(GuildContext db)
        {
            _db = db;
        }

        public async Task<Guild> Handle(GetGuildRequest request, CancellationToken cancellationToken)
        {
            Guild? guild =
                await _db.Guilds
                    .Include(g => g.Users)
                    .Include(g => g.Infractions)
                    .Include(g => g.Configuration)
                    .AsSplitQuery()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.Id == request.GuildId, cancellationToken);

            return guild;
        }
    }
}