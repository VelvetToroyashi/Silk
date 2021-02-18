using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Database.Models;

namespace Silk.Core.Database.MediatR.Handlers.Guilds
{
    public class GuildAddRequestHandler : IRequestHandler<AddGuildRequest, Guild>
    {
        private readonly SilkDbContext _db;
        
        public GuildAddRequestHandler(SilkDbContext db)
        {
            _db = db;
        }
        
        public async Task<Guild> Handle(AddGuildRequest request, CancellationToken cancellationToken)
        {
            var guild = new Guild { Id = request.GuildId, Configuration = new(), Prefix = Bot.DefaultCommandPrefix };
            await _db.SaveChangesAsync(cancellationToken);
            return guild;
        }
    }
}