using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Greetings;

public static class GetPendingGreetings
{
    public record Request : IRequest<IReadOnlyList<PendingGreetingEntity>>;
    
    internal class Handler : IRequestHandler<Request, IReadOnlyList<PendingGreetingEntity>>
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;
        
        public Handler(IDbContextFactory<GuildContext> dbFactory) => _dbFactory = dbFactory;


        public async Task<IReadOnlyList<PendingGreetingEntity>> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            return await db.PendingGreetings.ToArrayAsync(cancellationToken); // Should probably take a shard ID to limit the query to that shard
        }
    }

}