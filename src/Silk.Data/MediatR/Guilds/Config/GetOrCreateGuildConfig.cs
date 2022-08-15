using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Guilds;

public static class GetOrCreateGuildConfig
{
    /// <summary>
    /// Request for retrieving or creating a <see cref="GuildConfigEntity" />.
    /// </summary>
    /// <param name="GuildID">The Id of the Guild</param>
    /// <param name="Prefix">The prefix of the Guild</param>
    public sealed record Request(Snowflake GuildID, string Prefix) : IRequest<GuildConfigEntity>;

    /// <summary>
    /// The default handler for <see cref="Request" />.
    /// </summary>
    internal sealed class Handler : IRequestHandler<Request, GuildConfigEntity>
    {
        private readonly IMediator _mediator;

        public Handler(IMediator mediator) => _mediator = mediator;

        public async Task<GuildConfigEntity> Handle(Request request, CancellationToken cancellationToken)
        {
            GuildConfigEntity? guildConfig        = await _mediator.Send(new GetGuildConfig.Request(request.GuildID), cancellationToken);

            if (guildConfig is not null)
                return guildConfig;
        
            var response = await _mediator.Send(new GetOrCreateGuild.Request(request.GuildID, request.Prefix), cancellationToken);

            guildConfig = response.Configuration;

            return guildConfig;
        }
    }
}