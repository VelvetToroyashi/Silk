using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.DTOs.Guilds.Users;

namespace Silk.Data.MediatR.Users;

public static class GetMostRecentUser
{
    public record Request(Snowflake GuildID) : IRequest<User?>;
    
    internal class Handler : IRequestHandler<Request, User?>
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;
        public Handler(IDbContextFactory<GuildContext> dbFactory) => _dbFactory = dbFactory;

        public async Task<User?> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            var history = await db.Histories
                                   .Where(j => j.GuildID == request.GuildID)
                                   .OrderByDescending(h => h.Date)
                                   .FirstOrDefaultAsync(cancellationToken);

            if (history is null)
                return null; // No users?
            
            var user = await db.Users.FirstAsync(g => g.ID == history.UserID, cancellationToken);

            return user;
        }
    }
}