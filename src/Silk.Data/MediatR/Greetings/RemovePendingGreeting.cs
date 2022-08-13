using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Results;

namespace Silk.Data.MediatR.Greetings;

public static class RemovePendingGreeting
{
    public record Request(int ID) : IRequest<Result>;
    
    internal class Handler : IRequestHandler<Request, Result>
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;

        public Handler(IDbContextFactory<GuildContext> dbFactory) => _dbFactory = dbFactory;

        public async Task<Result> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            var greeting = await db.PendingGreetings.FirstOrDefaultAsync(x => x.Id == request.ID, cancellationToken);

            if (greeting is null)
                return Result.FromError(new NotFoundError());
            
            db.PendingGreetings.Remove(greeting);
            
            await db.SaveChangesAsync(cancellationToken);

            return Result.FromSuccess();
        }
    }
}