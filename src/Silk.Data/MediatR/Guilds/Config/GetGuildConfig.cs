using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using EFCoreSecondLevelCacheInterceptor;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Guilds;

public static class GetGuildConfig
{
    /// <summary>
    /// Request for getting the <see cref="GuildConfigEntity" /> for the Guild.
    /// </summary>
    /// <param name="GuildId">The Id of the Guild.</param>
    /// <param name="AsTracking">Whether to use tracking query behavior.</param>
    public sealed record Request(Snowflake GuildId, bool AsTracking = false) : IRequest<GuildConfigEntity>;

    /// <summary>
    /// The default handler for <see cref="Request" />.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class Handler : IRequestHandler<Request, GuildConfigEntity>
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;

        public Handler(IDbContextFactory<GuildContext> dbFactory) 
            => _dbFactory = dbFactory;

        public async ValueTask<GuildConfigEntity> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            //TODO: Add commands to get individual configs.
            var initialQueryable = db.GuildConfigs
                                     .Cacheable()
                                     .AsSplitQuery()
                                     .Include(g => g.Greetings)
                                     .Include(c => c.Invites)
                                     .Include(c => c.Invites.Whitelist)
                                     .Include(c => c.InfractionSteps)
                                     .Include(c => c.Exemptions)
                                     .Include(c => c.Logging)
                                     .Include(c => c.Logging.MemberJoins)
                                     .Include(c => c.Logging.MemberLeaves)
                                     .Include(c => c.Logging.MessageDeletes)
                                     .Include(c => c.Logging.MessageEdits)
                                     .Include(c => c.Logging.Infractions);

            var query = request.AsTracking ? initialQueryable.AsTracking() : initialQueryable.AsNoTracking();
            
           var ret = await query.FirstAsync(g => g.GuildID == request.GuildId, cancellationToken);
           return ret;
        }
    }
}
