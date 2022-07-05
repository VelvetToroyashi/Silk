using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Guilds;

public static class GetOrCreateGuild
{
    /// <summary>
    /// Request for retrieving or creating a <see cref="GuildEntity" />.
    /// </summary>
    /// <param name="GuildID">The Id of the Guild</param>
    /// <param name="Prefix">The prefix of the Guild</param>
    public sealed record Request(Snowflake GuildID, string Prefix) : IRequest<GuildEntity>;

    /// <summary>
    /// The default handler for <see cref="Request" />.
    /// </summary>
    internal sealed class Handler : IRequestHandler<Request, GuildEntity>
    {
        private readonly GuildContext _db;
        private readonly IMediator    _mediator;

        public Handler(GuildContext db, IMediator mediator)
        {
            _db       = db;
            _mediator = mediator;
        }

        public async Task<GuildEntity> Handle(Request request, CancellationToken cancellationToken)
        {
            GuildEntity? guild = await _db.Guilds.AsNoTracking().FirstOrDefaultAsync(g => g.ID == request.GuildID, cancellationToken);

            if (guild is not null)
                return guild;

            guild = await _mediator.Send(new AddGuild.Request(request.GuildID, request.Prefix), cancellationToken);

            return guild;
        }
    }
}