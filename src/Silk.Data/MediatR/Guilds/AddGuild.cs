using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Guilds;

public static class AddGuild
{
    /// <summary>
    /// Request for adding a <see cref="GuildEntity" /> to the database.
    /// </summary>
    /// <param name="GuildID">The Id of the Guild</param>
    /// <param name="Prefix">The prefix for the Guild</param>
    public sealed record Request(Snowflake GuildID, string Prefix) : IRequest<GuildEntity>;

    /// <summary>
    /// The default handler for <see cref="Request" />.
    /// </summary>
    internal sealed class Handler : IRequestHandler<Request, GuildEntity>
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public async Task<GuildEntity> Handle(Request request, CancellationToken cancellationToken)
        {
            
            
            var guild = await _db.Guilds.FirstOrDefaultAsync(g => g.ID == request.GuildID, cancellationToken);

            if (guild is not null)
                return guild;
            
            guild = new()
            {
                ID            = request.GuildID,
                Prefix        = request.Prefix,
                Configuration = new() { GuildID = request.GuildID },
            };

            _db.Guilds.Add(guild);
            
            await _db.SaveChangesAsync(cancellationToken);
            return guild;
        }
    }
}