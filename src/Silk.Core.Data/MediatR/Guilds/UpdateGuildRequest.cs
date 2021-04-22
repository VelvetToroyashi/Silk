using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR.Guilds
{
    /// <summary>
    ///     Request for updating a <see cref="Guild" />.
    /// </summary>
    /// <param name="GuildId">The Id of the Guild</param>
    /// <param name="Infraction">The infraction for this update (can pass null as argument if not needed)</param>
    public record UpdateGuildRequest(ulong GuildId, Infraction? Infraction) : IRequest;

    /// <summary>
    ///     The default handler for <see cref="UpdateGuildRequest" />
    /// </summary>
    public class UpdateGuildHandler : IRequestHandler<UpdateGuildRequest>
    {
        private readonly GuildContext _db;

        public UpdateGuildHandler(GuildContext db)
        {
            _db = db;
        }

        public async Task<Unit> Handle(UpdateGuildRequest request, CancellationToken cancellationToken)
        {
            Guild guild = await _db.Guilds.FirstAsync(g => g.Id == request.GuildId, cancellationToken);

            if (request.Infraction is not null)
            {
                guild.Infractions.Add(request.Infraction);
                await _db.SaveChangesAsync(cancellationToken);
            }

            return new();
        }
    }
}