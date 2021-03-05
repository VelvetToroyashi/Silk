using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.CompilerServices;
using Silk.Data.Models;

namespace Silk.Data.MediatR.Handlers
{
    public class GuildRequestHandler
    {
        public class GetHandler : IRequestHandler<GuildRequest.Get, Guild>
        {
            private readonly SilkDbContext _db;
            public GetHandler(SilkDbContext db)
            {
                _db = db;
            }

            public async Task<Guild> Handle(GuildRequest.Get request, CancellationToken cancellationToken)
            {
                Guild? guild =
                    await _db.Guilds
                    .Include(g => g.Users)
                    .Include(g => g.Infractions)
                    .AsSplitQuery()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.Id == request.GuildId, cancellationToken);
                
                return guild;
            }
        }

        public class UpdateHandler : IRequestHandler<GuildRequest.Update>
        {
            private readonly SilkDbContext _db;
            public UpdateHandler(SilkDbContext db)
            {
                _db = db;
            }

            public async Task<Unit> Handle(GuildRequest.Update request, CancellationToken cancellationToken)
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
        
        public class AddHandler : IRequestHandler<GuildRequest.Add, Guild>
        {
            private readonly SilkDbContext _db;

            public AddHandler(SilkDbContext db)
            {
                _db = db;
            }

            public async Task<Guild> Handle(GuildRequest.Add request, CancellationToken cancellationToken)
            {
                var guild = new Guild {Id = request.GuildId, Configuration = new(), Prefix = request.Prefix};
                await _db.SaveChangesAsync(cancellationToken);
                return guild;
            }
        }
        
        
        public class GetOrCreateHandler : IRequestHandler<GuildRequest.GetOrCreate, Guild>
        {
            private readonly SilkDbContext _db;

            public GetOrCreateHandler(SilkDbContext db)
            {
                _db = db;
            }
        
            public async Task<Guild> Handle(GuildRequest.GetOrCreate request, CancellationToken cancellationToken)
            {
                Guild? guild = await _db.Guilds
                    .Include(g => g.Users)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(g => g.Id == request.GuildId, cancellationToken);

                if (guild is not null) return guild;

                guild = new()
                {
                    Id = request.GuildId,
                    Users = new(),
                    Prefix = request.Prefix,
                    Configuration = new() { GuildId = request.GuildId }
                };
                _db.Guilds.Add(guild);
                await _db.SaveChangesAsync(cancellationToken);

                return guild;
            }
        }
    }
}