using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Remora.Results;
using Silk.Data.DTOs.Guilds.Config;

namespace Silk.Data.MediatR.Greetings;

public static class UpdateGuildGreeting
{
    public record Request(GuildGreeting Greeting) : IRequest<Result<GuildGreeting>>;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class Handler : IRequestHandler<Request, Result<GuildGreeting>>
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public async ValueTask<Result<GuildGreeting>> Handle(Request request, CancellationToken cancellationToken)
        {
            
            
            var existingGreeting = await _db.GuildGreetings
                                           .AsTracking()
                                           .FirstOrDefaultAsync(g => g.Id == request.Greeting.Id, cancellationToken);
            if (existingGreeting is null)
                return Result<GuildGreeting>.FromError(new NotFoundError("Greeting does not exist"));

            var updatedGreetingEntity = request.Greeting.Adapt(existingGreeting);
            var saved = await _db.SaveChangesAsync(cancellationToken) > 0;

            return saved 
                ? Result<GuildGreeting>.FromSuccess(updatedGreetingEntity.Adapt<GuildGreeting>())
                : Result<GuildGreeting>.FromError(new GenericError("Failed to update greeting"));
        }
    }
}