using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Guilds;

/// <summary>
///     Request for retrieving or creating a <see cref="GuildConfigEntity" />.
/// </summary>
/// <param name="GuildID">The Id of the Guild</param>
/// <param name="Prefix">The prefix of the Guild</param>
public record GetOrCreateGuildConfigRequest(Snowflake GuildID, string Prefix) : IRequest<GuildConfigEntity>;

/// <summary>
///     The default handler for <see cref="GetOrCreateGuildConfigRequest" />.
/// </summary>
public class GetOrCreateGuildConfigHandler : IRequestHandler<GetOrCreateGuildConfigRequest, GuildConfigEntity>
{
    private readonly IMediator _mediator;

    public GetOrCreateGuildConfigHandler(IMediator mediator) => _mediator = mediator;

    public async Task<GuildConfigEntity> Handle(GetOrCreateGuildConfigRequest request, CancellationToken cancellationToken)
    {
        var                guildConfigRequest = new GetGuildConfigRequest(request.GuildID);
        GuildConfigEntity? guildConfig        = await _mediator.Send(guildConfigRequest, cancellationToken);

        if (guildConfig is not null)
            return guildConfig;
        
        GuildEntity? response = await _mediator.Send(new GetOrCreateGuildRequest(request.GuildID, request.Prefix), cancellationToken);

        guildConfig = response.Configuration;

        return guildConfig;
    }
}