using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Database.Models;

namespace Silk.Core.Database.MediatR.Handlers
{
    public class GuildRequestHandler
    {
        public class GuildAddRequestHandler : IRequestHandler<GuildRequest.AddGuildRequest, Guild>
        {
            private readonly SilkDbContext _db;

            public GuildAddRequestHandler(SilkDbContext db)
            {
                _db = db;
            }

            public async Task<Guild> Handle(GuildRequest.AddGuildRequest request, CancellationToken cancellationToken)
            {
                var guild = new Guild {Id = request.GuildId, Configuration = new(), Prefix = Bot.DefaultCommandPrefix};
                await _db.SaveChangesAsync(cancellationToken);
                return guild;
            }
        }
        
        
        public class GuildGetOrCreateHandler : IRequestHandler<GuildRequest.GetOrCreateGuildRequest, Guild>
        {
            private readonly SilkDbContext _db;

            public GuildGetOrCreateHandler(SilkDbContext db)
            {
                _db = db;
            }
        
            public async Task<Guild> Handle(GuildRequest.GetOrCreateGuildRequest request, CancellationToken cancellationToken)
            {
                Guild? guild = await _db.Guilds
                    .Include(g => g.Users)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(g => g.Id == request.GuildId, cancellationToken);

                if (guild is not null) return guild;
                guild = new()
                {
                    Id = request.GuildId,
                    Prefix = Bot.DefaultCommandPrefix
                };
                _db.Attach(guild);
                await _db.SaveChangesAsync(cancellationToken);
                return guild;
            }
        }
    }
}