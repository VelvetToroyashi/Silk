using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Guilds.Config.Mod
{
    /// <summary>
    /// Request for retrieving or creating a <see cref="GuildModConfig" />.
    /// </summary>
    /// <param name="GuildId">The Id of the Guild</param>
    /// <param name="Prefix">The prefix of the Guild</param>
    public record GetOrCreateGuildModConfigRequest(ulong GuildId, string Prefix) : IRequest<GuildModConfig>;

    /// <summary>
    /// The default handler for <see cref="GetOrCreateGuildModConfigRequest" />.
    /// </summary>
    public class GetOrCreateGuildModConfigHandler : IRequestHandler<GetOrCreateGuildModConfigRequest, GuildModConfig>
    {
        private readonly IMediator _mediator;
        public GetOrCreateGuildModConfigHandler(IMediator mediator) => _mediator = mediator;

        public async Task<GuildModConfig> Handle(GetOrCreateGuildModConfigRequest configRequest, CancellationToken cancellationToken)
        {
            var guildModConfigRequest = new GetGuildModConfigRequest(configRequest.GuildId);
            GuildModConfig guildModConfig = await _mediator.Send(guildModConfigRequest, cancellationToken);

            if (guildModConfig is not null)
                return guildModConfig;

            var request = new GetOrCreateGuildRequest(configRequest.GuildId, configRequest.Prefix);
            var response = await _mediator.Send(request, cancellationToken);

            guildModConfig = response.ModConfig;

            return guildModConfig;
        }
    }
}