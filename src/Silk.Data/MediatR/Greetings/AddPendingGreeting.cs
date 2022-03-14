using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Greetings;

public static class AddPendingGreeting
{
    public record Request(Snowflake UserID, Snowflake GuildID, int GreetingID) : IRequest<Result<PendingGreetingEntity>>;
    
    internal class Handler : IRequestHandler<Request, Result<PendingGreetingEntity>>
    {
        private readonly GuildContext _context;

        public Handler(GuildContext context) => _context = context;

        public async Task<Result<PendingGreetingEntity>> Handle(Request request, CancellationToken cancellationToken)
        {
            var pendingGreeting = new PendingGreetingEntity
            {
                GreetingID = request.GreetingID,
                GuildID    = request.GuildID,
                UserID     = request.UserID
            };
            
            try
            {
                _context.PendingGreetings.Add(pendingGreeting);

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception e)
            {
                return Result<PendingGreetingEntity>.FromError(new ExceptionError(e));
            }
            
            return Result<PendingGreetingEntity>.FromSuccess(pendingGreeting);
        }
    }
}