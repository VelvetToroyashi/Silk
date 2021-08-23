using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Guilds
{
    /// <summary>
    ///     Request for retrieving or creating a <see cref="Guild" />.
    /// </summary>
    /// <param name="GuildId">The Id of the Guild</param>
    /// <param name="Prefix">The prefix of the Guild</param>
    public record GetOrCreateGuildRequest(ulong GuildId, string Prefix) : IRequest<Guild>;

    /// <summary>
    ///     The default handler for <see cref="GetOrCreateGuildRequest" />.
    /// </summary>
    public class GetOrCreateGuildHandler : IRequestHandler<GetOrCreateGuildRequest, Guild>
    {
        private readonly GuildContext _db;
        private IMediator _mediator;
        public GetOrCreateGuildHandler(GuildContext db, IMediator mediator)
        {
            _db = db;
            _mediator = mediator;
        }

        public async Task<Guild> Handle(GetOrCreateGuildRequest request, CancellationToken cancellationToken)
        {
            Guild? guild = await _db.Guilds
                .Include(g => g.Users)
                .Include(g => g.Infractions)
                .AsSplitQuery()
                .OrderBy(g => g.Id)
                .FirstOrDefaultAsync(g => g.Id == request.GuildId, cancellationToken);

            if (guild is not null)
                return guild;

            guild = await _mediator.Send(new AddGuildRequest(request.GuildId, request.Prefix), cancellationToken);
            
            return guild;
        }
    }
}