using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Guilds.Config;

/// <summary>
///     Request for retrieving or creating a <see cref="GuildModConfigEntity" />.
/// </summary>
/// <param name="GuildId">The Id of the Guild</param>
/// <param name="Prefix">The prefix of the Guild</param>
public record GetOrCreateGuildModConfigRequest(Snowflake GuildID, string Prefix) : IRequest<GuildModConfigEntity>;

/// <summary>
///     The default handler for <see cref="GetOrCreateGuildModConfigRequest" />.
/// </summary>
public class GetOrCreateGuildModConfigHandler : IRequestHandler<GetOrCreateGuildModConfigRequest, GuildModConfigEntity>
{
    private readonly IMediator _mediator;
    public GetOrCreateGuildModConfigHandler(IMediator mediator) => _mediator = mediator;

    public async Task<GuildModConfigEntity> Handle(GetOrCreateGuildModConfigRequest configRequest, CancellationToken cancellationToken)
    {
        GuildModConfigEntity? guildModConfig = await _mediator.Send(new GetGuildModConfigRequest(configRequest.GuildID), cancellationToken);

        if (guildModConfig is not null)
            return guildModConfig;

        var          request  = new GetOrCreateGuildRequest(configRequest.GuildID, configRequest.Prefix);
        GuildEntity? response = await _mediator.Send(request, cancellationToken);

        guildModConfig = response.ModConfig;

        return guildModConfig;
    }
}