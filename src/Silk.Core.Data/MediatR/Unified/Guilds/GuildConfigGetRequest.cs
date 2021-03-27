using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Unified.Guilds
{
    public record GuildConfigGetRequest(ulong GuildId) : IRequest<GuildConfig>;

    public class GuildConfigGetHandler
    {
        private readonly GuildContext _db;

        public GuildConfigGetHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<GuildConfig> Handle(GuildConfigGetRequest request, CancellationToken cancellationToken)
        {
            GuildConfig config =
                await _db.GuildConfigs
                    .Include(c => c.InfractionSteps)
                    .Include(c => c.DisabledCommands)
                    //.Include(c => c.BlackListedWords)
                    .Include(c => c.SelfAssignableRoles)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(g => g.GuildId == request.GuildId, cancellationToken);
            return config;
        }
    }
}