using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Handlers
{
    public class GuildRequestHandler
    {
        public class GetHandler : IRequestHandler<GuildRequest.Get, Guild>
        {
            private readonly GuildContext _db;
            public GetHandler(GuildContext db)
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
            private readonly GuildContext _db;
            public UpdateHandler(GuildContext db)
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
            private readonly GuildContext _db;

            public AddHandler(GuildContext db)
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
            private readonly GuildContext _db;

            public GetOrCreateHandler(GuildContext db)
            {
                _db = db;
            }
        
            public async Task<Guild> Handle(GuildRequest.GetOrCreate request, CancellationToken cancellationToken)
            {
                Guild? guild = await _db.Guilds
                    .Include(g => g.Users)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(g => g.Id == request.GuildId, cancellationToken);
    
                if ((Guild?)guild is not null) 
                    return guild;

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