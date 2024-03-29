﻿using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using EFCoreSecondLevelCacheInterceptor;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Guilds;

public static class GetGuild
{
    /// <summary>
    /// Request for retrieving a <see cref="GuildEntity" />.
    /// </summary>
    /// <param name="GuildID">The Id of the Guild</param>
    public sealed record Request(Snowflake GuildID) : IRequest<GuildEntity?>;

    /// <summary>
    /// The default handler for <see cref="Request" />.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class Handler : IRequestHandler<Request, GuildEntity?>
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;

        public Handler(IDbContextFactory<GuildContext> dbFactory) 
            => _dbFactory = dbFactory;


        public async ValueTask<GuildEntity?> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            var guild = await db.Guilds.Cacheable().FirstOrDefaultAsync(g => g.ID == request.GuildID, cancellationToken);

            return guild;
        }
    }
}