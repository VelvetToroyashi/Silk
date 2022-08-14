using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Guilds;

public static class UpdateGuild
{
    // Create a public Request record that extends IRequest<Guild, Result<Guild>> and an internal handler class that implements IRequestHandler<Request, Result<Guild>>
    
    public sealed record Request(Snowflake GuildID, string Prefix) : IRequest<Result<GuildEntity>>;

    internal sealed class Handler : IRequestHandler<Request, Result<GuildEntity>>
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;
        public Handler(IDbContextFactory<GuildContext> dbFactory) => _dbFactory = dbFactory;

        public async Task<Result<GuildEntity>> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            var guild = await db.Guilds.FirstOrDefaultAsync(g => g.ID == request.GuildID, cancellationToken);
            
            if (guild is null)
                return Result<GuildEntity>.FromError(new NotFoundError($"No guild was found with the ID of {request.GuildID}"));
            
            guild.Prefix = request.Prefix;
            
            await db.SaveChangesAsync(cancellationToken);
            
            return Result<GuildEntity>.FromSuccess(guild);
        }
    }
}