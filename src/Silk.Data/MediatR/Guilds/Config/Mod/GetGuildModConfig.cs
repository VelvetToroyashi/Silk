using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Guilds.Config;

public static class GetGuildModConfig
{
    public sealed record Request(Snowflake GuildId) : IRequest<GuildModConfigEntity>;

    internal sealed class Handler : IRequestHandler<Request, GuildModConfigEntity>
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public Task<GuildModConfigEntity> Handle(Request request, CancellationToken cancellationToken)
        {
            return _db.GuildModConfigs
                      .Include(c => c.Invites)
                      .Include(c => c.Invites.Whitelist)
                      .Include(c => c.InfractionSteps)
                      .Include(c => c.Exemptions)
                      .Include(c => c.Logging)
                      .Include(c => c.Logging.MemberJoins)
                      .Include(c => c.Logging.MemberLeaves)
                      .Include(c => c.Logging.MessageDeletes)
                      .Include(c => c.Logging.MessageEdits)
                      .FirstOrDefaultAsync(c => c.GuildID == request.GuildId, cancellationToken)!;
        }
    }
}