using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Guilds;

/// <summary>
///     Request for adding a <see cref="GuildEntity" /> to the database.
/// </summary>
/// <param name="GuildID">The Id of the Guild</param>
/// <param name="Prefix">The prefix for the Guild</param>
public sealed record AddGuildRequest(Snowflake GuildID, string Prefix) : IRequest<GuildEntity>;

/// <summary>
///     The default handler for <see cref="AddGuildRequest" />.
/// </summary>
public sealed class AddGuildHandler : IRequestHandler<AddGuildRequest, GuildEntity>
{
    private readonly GuildContext _db;
    public AddGuildHandler(GuildContext db) => _db = db;

    public async Task<GuildEntity> Handle(AddGuildRequest request, CancellationToken cancellationToken)
    {
        GuildEntity guild = new()
        {
            Id            = request.GuildID,
            Prefix        = request.Prefix,
            Configuration = new() { GuildID = request.GuildID },
            ModConfig     = new() { GuildID = request.GuildID },
        };

        _db.Guilds.Add(guild);
        await _db.SaveChangesAsync(cancellationToken);

        return guild;
    }
}