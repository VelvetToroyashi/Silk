using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Guilds
{
    public record GuildUpdateRequest(ulong GuildId, Infraction? Infraction) : IRequest;

    public class GuildUpdateHandler : IRequestHandler<GuildUpdateRequest>
    {
        private readonly GuildContext _db;
        public GuildUpdateHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<Unit> Handle(GuildUpdateRequest request, CancellationToken cancellationToken)
        {
            Guild guild = await _db.Guilds.FirstAsync(g => g.Id == request.GuildId, cancellationToken);

            if (request.Infraction is not null)
            {
                guild.Infractions.Add(request.Infraction);
                await _db.SaveChangesAsync(cancellationToken);
            }
            return new();
        }
    }
}