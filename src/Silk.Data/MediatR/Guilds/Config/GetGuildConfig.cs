using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Guilds;

public static class GetGuildConfig
{
    /// <summary>
    /// Request for getting the <see cref="GuildConfigEntity" /> for the Guild.
    /// </summary>
    /// <param name="GuildId">The Id of the Guild</param>
    public sealed record Request(Snowflake GuildId, bool AsTracking = false) : IRequest<GuildConfigEntity?>;

    /// <summary>
    /// The default handler for <see cref="Request" />.
    /// </summary>
    internal sealed class Handler : IRequestHandler<Request, GuildConfigEntity?>
    {
        private readonly GuildContext _db;
        
        public Handler(GuildContext db) => _db = db;

        public async Task<GuildConfigEntity?> Handle(Request request, CancellationToken cancellationToken)
        {
            //TODO: Add commands to get individual configs.
            var initialQueryable = _db.GuildConfigs
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

            return request.AsTracking
                ? await initialQueryable.AsTracking()
                                        .FirstOrDefaultAsync(g => g.GuildID == request.GuildId, cancellationToken)
                : await initialQueryable.AsNoTracking()
                                        .FirstOrDefaultAsync(g => g.GuildID == request.GuildId, cancellationToken);
        }
    }
}
