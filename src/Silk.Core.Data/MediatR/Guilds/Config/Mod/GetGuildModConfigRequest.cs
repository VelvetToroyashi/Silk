using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR.Guilds.Config;

public sealed record GetGuildModConfigRequest(Snowflake GuildID) : IRequest<GuildModConfigEntity?>;

public sealed class GetGuildModConfigHandler : IRequestHandler<GetGuildModConfigRequest, GuildModConfigEntity?>
{
    private readonly GuildContext _db;
    public GetGuildModConfigHandler(GuildContext db) => _db = db;

    public Task<GuildModConfigEntity?> Handle(GetGuildModConfigRequest request, CancellationToken cancellationToken)
    {
        return _db.GuildModConfigs
                  .Include(c => c.AllowedInvites)
                  .Include(c => c.InfractionSteps)
                  .Include(c => c.Exemptions)
                  .FirstOrDefaultAsync(c => c.GuildID == request.GuildID, cancellationToken)!;
    }
}