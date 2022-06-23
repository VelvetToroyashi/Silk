using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
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
            try
            {
                _db.Histories.Add(new() { UserID = request.UserID, GuildID = request.GuildID, Date = request.Date, IsJoin = true });
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                return new NotFoundError($"There is no user by the ID of {request.UserID} on {request.GuildID}.");
            }

            return Result.FromSuccess();
        }
    }
}