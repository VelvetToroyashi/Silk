using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Guilds;

/// <summary>
///     Request for retrieving or creating a <see cref="GuildConfigEntity" />.
/// </summary>
/// <param name="GuildId">The Id of the Guild</param>
/// <param name="Prefix">The prefix of the Guild</param>
public record GetOrCreateGuildConfigRequest(ulong GuildId, string Prefix) : IRequest<GuildConfigEntity>;

/// <summary>
///     The default handler for <see cref="GetOrCreateGuildConfigRequest" />.
/// </summary>
public class GetOrCreateGuildConfigHandler : IRequestHandler<GetOrCreateGuildConfigRequest, GuildConfigEntity>
{
    private readonly IMediator _mediator;

    public GetOrCreateGuildConfigHandler(IMediator mediator) => _mediator = mediator;

    public async Task<GuildConfigEntity> Handle(GetOrCreateGuildConfigRequest configRequest, CancellationToken cancellationToken)
    {
        var                guildConfigRequest = new GetGuildConfigRequest(configRequest.GuildId);
        GuildConfigEntity? guildConfig        = await _mediator.Send(guildConfigRequest, cancellationToken);

        if (guildConfig is not null)
            return guildConfig;

        var          request  = new GetOrCreateGuildRequest(configRequest.GuildId, configRequest.Prefix);
        GuildEntity? response = await _mediator.Send(request, cancellationToken);

        guildConfig = response.Configuration;

        return guildConfig;
    }
}