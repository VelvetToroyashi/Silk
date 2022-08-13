using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Results;
using Silk.Data.DTOs.Guilds.Config;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Greetings;

public static class AddGuildGreeting
{
    public record Request(GuildGreeting Greeting) : IRequest<Result<GuildGreeting>>;

    internal class Handler : IRequestHandler<Request, Result<GuildGreeting>>
    {
        private readonly GuildContext _db;

        public Handler(GuildContext db) 
            => _db = db;

        public async Task<Result<GuildGreeting>> Handle(Request request, CancellationToken cancellationToken)
        {
            var guildConfig = await _db.GuildConfigs
                                       .Include(gc => gc.Greetings)
                                       .FirstOrDefaultAsync(gc => gc.GuildID == request.Greeting.GuildID, cancellationToken);
            if (guildConfig is null)
                return Result<GuildGreeting>.FromError(new NotFoundError("Guild config not found"));

            var existingGreeting = guildConfig.Greetings
                                              .FirstOrDefault(g => g.Id == request.Greeting.Id);
            if (existingGreeting is not null)
                return Result<GuildGreeting>.FromError(new GenericError("Greeting already exists"));

            // If no greeting exists, create a new one but make sure the ID is not set
            var newGreeting = (request.Greeting with { Id = 0 }).Adapt<GuildGreetingEntity>();

            guildConfig.Greetings.Add(newGreeting);
            var saved = await _db.SaveChangesAsync(cancellationToken) > 0;

            return saved 
                ? Result<GuildGreeting>.FromSuccess(newGreeting.Adapt<GuildGreeting>())
                : Result<GuildGreeting>.FromError(new GenericError("Failed to save greeting"));
        }
    }
}