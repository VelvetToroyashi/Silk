using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;

namespace Silk.Data.MediatR.Users;

public static class SetUserTimezone
{
    /// <summary>
    /// Sets a user's timezone.
    /// </summary>
    /// <param name="UserID">The ID of the user.</param>
    /// <param name="TimezoneID">The offset ID (e.g. America/New_York).</param>
    /// <param name="ShareTimezone">Whether the user want their timezone to be shared publicly</param>
    public record Request(Snowflake UserID, string TimezoneID, bool? ShareTimezone = null) : IRequest;

    internal class Handler : IRequestHandler<Request>
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;
        public Handler(IDbContextFactory<GuildContext> dbFactory) => _dbFactory = dbFactory;
        
        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            var user = await db.Users.FirstOrDefaultAsync(u => u.ID == request.UserID, cancellationToken);

            if (user is null)
                return Unit.Value; // TODO: Return result?

            user.TimezoneID    = request.TimezoneID;
            user.ShareTimezone = request.ShareTimezone ?? user.ShareTimezone;

            await db.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}