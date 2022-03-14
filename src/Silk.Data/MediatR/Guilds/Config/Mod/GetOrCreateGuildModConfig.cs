using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Guilds.Config;

public static class GetOrCreateGuildModConfig
{
    /// <summary>
    /// Request for retrieving or creating a <see cref="GuildModConfigEntity" />.
    /// </summary>
    /// <param name="GuildID">The Id of the Guild</param>
    /// <param name="Prefix">The prefix of the Guild</param>
    public sealed record Request(Snowflake GuildID, string Prefix) : IRequest<GuildModConfigEntity>;

    /// <summary>
    /// The default handler for <see cref="Request" />.
    /// </summary>
    internal sealed class Handler : IRequestHandler<Request, GuildModConfigEntity>
    {
        private readonly IMediator _mediator;
        public Handler(IMediator mediator) => _mediator = mediator;

        public async Task<GuildModConfigEntity> Handle(Request configRequest, CancellationToken cancellationToken)
        {
            GuildModConfigEntity? guildModConfig = await _mediator.Send(new GetGuildModConfig.Request(configRequest.GuildID), cancellationToken);

            if (guildModConfig is not null)
                return guildModConfig;

            var          request  = new GetOrCreateGuild.Request(configRequest.GuildID, configRequest.Prefix);
            GuildEntity? response = await _mediator.Send(request, cancellationToken);

            guildModConfig = response.ModConfig;

            return guildModConfig;
        }
    }
}