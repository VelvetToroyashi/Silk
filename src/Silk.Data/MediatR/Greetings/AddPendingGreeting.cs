using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Greetings;

public static class AddPendingGreeting
{
    public record Request(Snowflake UserID, Snowflake GuildID, int GreetingID) : IRequest<Result<PendingGreetingEntity>>;
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class Handler : IRequestHandler<Request, Result<PendingGreetingEntity>>
    {
        private readonly GuildContext _db;

        public Handler(GuildContext db) => _db = db;

        public async ValueTask<Result<PendingGreetingEntity>> Handle(Request request, CancellationToken cancellationToken)
        {
            var pendingGreeting = new PendingGreetingEntity // Why not an .Adapt<T> here?
            {
                GreetingID = request.GreetingID,
                GuildID    = request.GuildID,
                UserID     = request.UserID
            };
            
            
            try
            {
                _db.PendingGreetings.Add(pendingGreeting);

                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception e)
            {
                return Result<PendingGreetingEntity>.FromError(new ExceptionError(e));
            }
            
            return Result<PendingGreetingEntity>.FromSuccess(pendingGreeting);
        }
    }
}