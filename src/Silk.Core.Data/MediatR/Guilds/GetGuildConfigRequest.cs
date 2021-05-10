using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Guilds
{
    /// <summary>
    ///     Request for getting the <see cref="GuildConfig" /> for the Guild.
    /// </summary>
    /// <param name="GuildId">The Id of the Guild</param>
    public record GetGuildConfigRequest(ulong GuildId) : IRequest<GuildConfig>;

    /// <summary>
    ///     The default handler for <see cref="GetGuildConfigRequest" />.
    /// </summary>
    public class GetGuildConfigHandler : IRequestHandler<GetGuildConfigRequest, GuildConfig>
    {
        private readonly GuildContext _db;
        public GetGuildConfigHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<GuildConfig> Handle(GetGuildConfigRequest request, CancellationToken cancellationToken)
        {
            GuildConfig config =
                await _db.GuildConfigs
                    .Include(c => c.InfractionSteps)
                    .Include(c => c.DisabledCommands)
                    //.Include(c => c.BlackListedWords)
                    .Include(c => c.RoleMenus)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(g => g.GuildId == request.GuildId, cancellationToken);
            return config;
        }
    }
}