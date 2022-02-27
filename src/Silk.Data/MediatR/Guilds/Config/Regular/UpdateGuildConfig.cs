using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Guilds;

public static class UpdateGuildConfig
{
    public record Request(Snowflake GuildID, List<GuildGreetingEntity> Greetings) : IRequest<GuildConfigEntity>;
    
    internal class Handler : IRequestHandler<Request, GuildConfigEntity>
    {
        private readonly GuildContext _db;

        public Handler(GuildContext db) => _db = db;

        public async Task<GuildConfigEntity> Handle(Request request, CancellationToken cancellationToken)
        {
            var config = await _db
                              .GuildConfigs
                              .Include(g => g.Greetings)
                              .FirstAsync(c => c.GuildID == request.GuildID, cancellationToken);
            
            _db.RemoveRange(config.Greetings.Except(request.Greetings));
            
            config.Greetings = request.Greetings;
            
            await _db.SaveChangesAsync(cancellationToken);
            
            return config;
        }
    }
}