using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Guilds.Config;

/// <summary>
///     Request for retrieving or creating a <see cref="GuildModConfigEntity" />.
/// </summary>
/// <param name="GuildId">The Id of the Guild</param>
/// <param name="Prefix">The prefix of the Guild</param>
public record GetOrCreateGuildModConfigRequest(ulong GuildId, string Prefix) : IRequest<GuildModConfigEntity>;

/// <summary>
///     The default handler for <see cref="GetOrCreateGuildModConfigRequest" />.
/// </summary>
public class GetOrCreateGuildModConfigHandler : IRequestHandler<GetOrCreateGuildModConfigRequest, GuildModConfigEntity>
{
	private readonly IMediator _mediator;
	public GetOrCreateGuildModConfigHandler(IMediator mediator) => _mediator = mediator;

	public async Task<GuildModConfigEntity> Handle(GetOrCreateGuildModConfigRequest configRequest, CancellationToken cancellationToken)
	{
		var guildModConfigRequest = new GetGuildModConfigRequest(configRequest.GuildId);
		GuildModConfigEntity? guildModConfig = await _mediator.Send(guildModConfigRequest, cancellationToken);

		if (guildModConfig is not null)
			return guildModConfig;

		var request = new GetOrCreateGuildRequest(configRequest.GuildId, configRequest.Prefix);
		GuildEntity? response = await _mediator.Send(request, cancellationToken);

		guildModConfig = response.ModConfig;

		return guildModConfig;
	}
}