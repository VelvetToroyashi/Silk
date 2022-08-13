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
        private readonly IDbContextFactory<GuildContext> _dbFactory;
        public Handler(IDbContextFactory<GuildContext> dbFactory) => _dbFactory = dbFactory;

        public async Task<Result> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            try
            {
                await db.Histories
                         .Upsert(new() { UserID = request.UserID, GuildID = request.GuildID, Date = request.Date, IsJoin = true })
                         .NoUpdate()
                         .RunAsync(cancellationToken);
            }
            catch
            {
                return new NotFoundError($"There is no user by the ID of {request.UserID} on {request.GuildID}.");
            }

            return Result.FromSuccess();
        }
    }
}