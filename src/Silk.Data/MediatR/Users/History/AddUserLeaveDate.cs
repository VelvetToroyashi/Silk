using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Remora.Results;

namespace Silk.Data.MediatR.Users.History;

public static class AddUserLeaveDate
{
    public record Request(Snowflake GuildID, Snowflake UserID, DateTimeOffset Date) : IRequest<Result>;
    
    internal class Handler : IRequestHandler<Request, Result>
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public async Task<Result> Handle(Request request, CancellationToken cancellationToken)
        {
            var history = await _db.Histories.LastOrDefaultAsync(h => h.UserID == request.UserID && h.GuildID == request.GuildID, cancellationToken);
            
            if (history is null)
                return new NotFoundError($"No user exists by the ID of {request.UserID} in guild {request.GuildID}");

            _db.Update(history);
            
            await _db.SaveChangesAsync(cancellationToken);

            return Result.FromSuccess();
        }
    }
}