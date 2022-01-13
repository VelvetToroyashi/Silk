using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Guilds.Config;

public static class GetGuildModConfig
{
    public sealed record Request(Snowflake GuildId) : IRequest<GuildModConfigEntity?>;

    internal sealed class Handler : IRequestHandler<Request, GuildModConfigEntity?>
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public Task<GuildModConfigEntity?> Handle(Request request, CancellationToken cancellationToken)
        {
            return _db.GuildModConfigs
                      .Include(c => c.AllowedInvites)
                      .Include(c => c.InfractionSteps)
                      .Include(c => c.Exemptions)
                      .Include(c => c.LoggingConfig)
                      .Include(c => c.LoggingConfig.MemberJoins)
                      .Include(c => c.LoggingConfig.MemberLeaves)
                      .Include(c => c.LoggingConfig.MessageDeletes)
                      .Include(c => c.LoggingConfig.MessageEdits)
                      .FirstOrDefaultAsync(c => c.GuildID == request.GuildId, cancellationToken)!;
        }
    }
}