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


        public GetOrCreateGuildHandler(GuildContext db)
        {
            _db = db;

        }

        public async Task<Guild> Handle(GetOrCreateGuildRequest request, CancellationToken cancellationToken)
        {
            Guild? guild = await _db.Guilds
                .Include(g => g.Users)
                .Include(g => g.Infractions)
                .Include(g => g.Configuration)
                .AsSplitQuery()
                .FirstOrDefaultAsync(g => g.Id == request.GuildId, cancellationToken);

            if (guild is not null)
                return guild;

            guild = new()
            {
                Id = request.GuildId,
                Users = new(),
                Prefix = request.Prefix,
                Configuration = new() {GuildId = request.GuildId}
            };
            _db.Guilds.Add(guild);
            await _db.SaveChangesAsync(cancellationToken);

            return guild;
        }
    }
}