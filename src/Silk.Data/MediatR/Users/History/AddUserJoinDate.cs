using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Remora.Results;

namespace Silk.Data.MediatR.Users.History;

public static class AddUserJoinDate
{
    public record Request(Snowflake GuildID, Snowflake UserID, DateTimeOffset Date) : IRequest<Result>;
    
    internal class Handler : IRequestHandler<Request, Result>
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public async Task<Result> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                                .Include(u => u.History)
                                .FirstOrDefaultAsync(u => u.ID == request.UserID, cancellationToken);
            
            if (user is null)
                return new NotFoundError($"No user exists by the ID of {request.UserID} in guild {request.GuildID}");

            user.History.Add(new () { UserID = user.ID, GuildID = request.GuildID, JoinDate = request.Date });

            _db.Update(user);
            
            await _db.SaveChangesAsync(cancellationToken);

            await _db.DisposeAsync();
            
            return Result.FromSuccess();
        }
    }
}