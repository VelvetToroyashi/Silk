using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Guilds
{
    public record GuildAddRequest(ulong GuildId, string Prefix) : IRequest<Guild>;

    public class GuildAddHandler : IRequestHandler<GuildAddRequest, Guild>
    {
        private readonly GuildContext _db;

        public GuildAddHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<Guild> Handle(GuildAddRequest request, CancellationToken cancellationToken)
        {
            var guild = new Guild {Id = request.GuildId, Configuration = new(), Prefix = request.Prefix};

            await _db.SaveChangesAsync(cancellationToken);
            return guild;
        }
    }
}