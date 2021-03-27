using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Guilds
{
    /// <summary>
    /// Request for retrieving a <see cref="Guild"/>.
    /// </summary>
    /// <param name="GuildId">The Id of the Guild</param>
    public record GetGuildRequest(ulong GuildId) : IRequest<Guild>;

    /// <summary>
    /// The default handler for <see cref="GetGuildRequest"/>.
    /// </summary>
    public class GetGuildHandler : IRequestHandler<GetGuildRequest, Guild>
    {
        private readonly GuildContext _db;

        public GetGuildHandler(GuildContext db)
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