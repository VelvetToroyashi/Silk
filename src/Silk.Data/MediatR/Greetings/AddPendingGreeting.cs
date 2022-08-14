using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Greetings;

public static class AddPendingGreeting
{
    public record Request(Snowflake UserID, Snowflake GuildID, int GreetingID) : IRequest<Result<PendingGreetingEntity>>;
    
    internal class Handler : IRequestHandler<Request, Result<PendingGreetingEntity>>
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;

        public Handler(IDbContextFactory<GuildContext> dbFactory) => _dbFactory = dbFactory;

        public async Task<Result<PendingGreetingEntity>> Handle(Request request, CancellationToken cancellationToken)
        {
            var pendingGreeting = new PendingGreetingEntity // Why not an .Adapt<T> here?
            {
                GreetingID = request.GreetingID,
                GuildID    = request.GuildID,
                UserID     = request.UserID
            };
            
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            try
            {
                db.PendingGreetings.Add(pendingGreeting);

                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception e)
            {
                return Result<PendingGreetingEntity>.FromError(new ExceptionError(e));
            }
            
            return Result<PendingGreetingEntity>.FromSuccess(pendingGreeting);
        }
    }
}