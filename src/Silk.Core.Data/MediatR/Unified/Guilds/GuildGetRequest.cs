using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Guilds
{
    public record GuildGetRequest(ulong GuildId) : IRequest<Guild>;

    public class GuildGetHandler : IRequestHandler<GuildGetRequest, Guild>
    {
        private readonly GuildContext _db;

        public GuildGetHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<Guild> Handle(GuildGetRequest request, CancellationToken cancellationToken)
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