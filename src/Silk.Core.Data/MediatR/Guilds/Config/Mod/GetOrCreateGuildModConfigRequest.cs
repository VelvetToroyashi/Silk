using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Guilds.Config.Mod
{
    /// <summary>
    /// Request for retrieving or creating a <see cref="GuildModConfig" />.
    /// </summary>
    /// <param name="GuildId">The Id of the Guild</param>
    public record GetOrCreateGuildModConfigRequest(ulong GuildId, string Prefix) : IRequest<GuildModConfig>;

    /// <summary>
    /// The default handler for <see cref="GetOrCreateGuildModConfigRequest" />.
    /// </summary>
    public class GetOrCreateGuildModConfigHandler : IRequestHandler<GetOrCreateGuildModConfigRequest, GuildModConfig>
    {
        private readonly GuildContext _db;
        private readonly IMediator _mediator;

        public GetOrCreateGuildModConfigHandler(GuildContext db, IMediator mediator)
        {
            _db = db;
            _mediator = mediator;
        }

        public async Task<GuildModConfig> Handle(GetOrCreateGuildModConfigRequest configRequest, CancellationToken cancellationToken)
        {
            GuildModConfig? guildModConfig = await _db.GuildModConfigs
                .Include(g => g.AllowedInvites)
                .Include(g => g.InfractionSteps)
                .FirstOrDefaultAsync(g => g.GuildId == configRequest.GuildId, cancellationToken);

            if (guildModConfig is not null)
                return guildModConfig;

            var request = new GetOrCreateGuildRequest(configRequest.GuildId, configRequest.Prefix);
            var response = await _mediator.Send(request, cancellationToken);

            guildModConfig = response.ModConfig;

            return guildModConfig;
        }
    }
}