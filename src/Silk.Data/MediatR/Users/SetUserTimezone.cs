using System.Linq;
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
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.ID == request.UserID, cancellationToken);

            if (user is null)
                return Unit.Value; // TODO: Return result?

            user.TimezoneID    = request.TimezoneID;
            user.ShareTimezone = request.ShareTimezone ?? user.ShareTimezone;

            await _db.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}