using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.DTOs.Guilds.Config;

namespace Silk.Data.MediatR.Greetings;

public static class GetGuildGreetings
{
    public record Request(Snowflake GuildID) : IRequest<List<GuildGreeting>>;

    internal class Handler : IRequestHandler<Request, List<GuildGreeting>>
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;
        
        public Handler(IDbContextFactory<GuildContext> dbFactory) => _dbFactory = dbFactory;

        public async Task<List<GuildGreeting>> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            return await db.GuildGreetings
                           .Where(g => g.GuildID == request.GuildID)
                           .ProjectToType<GuildGreeting>()
                           .ToListAsync(cancellationToken: cancellationToken);
        }
    }
}