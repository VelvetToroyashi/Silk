using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Remora.Results;

namespace Silk.Data.MediatR.Guilds;

public static class ShedGuilds
{
    public record Request(int ShardID, int ShardCount, IReadOnlyList<Snowflake> GuildIDs) : IRequest<Result<int>>;
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class Handler : IRequestHandler<Request, Result<int>>
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;

        public Handler(IDbContextFactory<GuildContext> dbFactory) 
            => _dbFactory = dbFactory;

        public async ValueTask<Result<int>> Handle(Request request, CancellationToken cancellationToken)
        {
            try
            {
                await using var db  = await _dbFactory.CreateDbContextAsync(cancellationToken);
                
                var ids = request.GuildIDs.Select(id => id.Value).ToArray();
                
                var deleted = await db.Database.ExecuteSqlRawAsync
                (
                 $"DELETE FROM guilds g WHERE ((g.\"id\"::bigint >> 22) % {request.ShardCount} = {request.ShardID}) AND g.\"id\" NOT IN({string.Join(", ", ids)}) ;",
                 cancellationToken: cancellationToken
                );
                
                return Result<int>.FromSuccess(deleted);
            }
            catch (Exception e)
            {
                return e;
            }
        }
    }
}