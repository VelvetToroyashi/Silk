using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.DTOs.Guilds.Users;

namespace Silk.Data.MediatR.Users;

public static class GetMostRecentUser
{
    public record Request(Snowflake GuildID) : IRequest<User?>;
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class Handler : IRequestHandler<Request, User?>
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public async ValueTask<User?> Handle(Request request, CancellationToken cancellationToken)
        {
            
            
            var history = await _db.Histories
                                   .Where(j => j.GuildID == request.GuildID)
                                   .OrderByDescending(h => h.Date)
                                   .FirstOrDefaultAsync(cancellationToken);

            if (history is null)
                return null; // No users?
            
            var user = await _db.Users.FirstAsync(g => g.ID == history.UserID, cancellationToken);

            return user;
        }
    }
}