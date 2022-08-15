using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Results;
using Silk.Data.DTOs.Guilds.Config;

namespace Silk.Data.MediatR.Greetings;

public static class UpdateGuildGreeting
{
    public record Request(GuildGreeting Greeting) : IRequest<Result<GuildGreeting>>;

    internal class Handler : IRequestHandler<Request, Result<GuildGreeting>>
    {
        private readonly IDbContextFactory<GuildContext> _dbFactory;
        public Handler(IDbContextFactory<GuildContext> dbFactory) => _dbFactory = dbFactory;

        public async Task<Result<GuildGreeting>> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            
            var existingGreeting = await db.GuildGreetings
                                           .AsTracking()
                                           .FirstOrDefaultAsync(g => g.Id == request.Greeting.Id, cancellationToken);
            if (existingGreeting is null)
                return Result<GuildGreeting>.FromError(new NotFoundError("Greeting does not exist"));

            var updatedGreetingEntity = request.Greeting.Adapt(existingGreeting);
            var saved = await db.SaveChangesAsync(cancellationToken) > 0;

            return saved 
                ? Result<GuildGreeting>.FromSuccess(updatedGreetingEntity.Adapt<GuildGreeting>())
                : Result<GuildGreeting>.FromError(new GenericError("Failed to update greeting"));
        }
    }
}