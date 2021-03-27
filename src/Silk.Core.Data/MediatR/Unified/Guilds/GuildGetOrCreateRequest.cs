using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Guilds
{
    public record GuildGetOrCreateRequest(ulong GuildId, string Prefix) : IRequest<Guild>;

    public class GuildGetOrCreateHandler : IRequestHandler<GuildGetOrCreateRequest, Guild>
    {
        private readonly GuildContext _db;

        public GuildGetOrCreateHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<Guild> Handle(GuildGetOrCreateRequest request, CancellationToken cancellationToken)
        {
            Guild? guild = await _db.Guilds
                .Include(g => g.Users)
                .AsSplitQuery()
                .FirstOrDefaultAsync(g => g.Id == request.GuildId, cancellationToken);

            if ((Guild?) guild is not null)
                return guild;

            guild = new()
            {
                Id = request.GuildId,
                Users = new(),
                Prefix = request.Prefix,
                Configuration = new() {GuildId = request.GuildId}
            };
            _db.Guilds.Add(guild);
            await _db.SaveChangesAsync(cancellationToken);

            return guild;
        }
    }
}