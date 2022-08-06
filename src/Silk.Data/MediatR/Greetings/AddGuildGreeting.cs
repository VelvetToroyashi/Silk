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
    public record Request(GuildGreetingDTO GreetingDto) : IRequest<Result<GuildGreetingDTO>>;

    internal class Handler : IRequestHandler<Request, Result<GuildGreetingDTO>>
    {
        private readonly IDbContextFactory<GuildContext> _dbContextFactory;

        public Handler(IDbContextFactory<GuildContext> dbContextFactory) 
            => _dbContextFactory = dbContextFactory;

        public async Task<Result<GuildGreetingDTO>> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var guildConfig = await dbContext.GuildConfigs
                                             .Include(gc => gc.Greetings)
                                             .FirstOrDefaultAsync(gc => gc.GuildID == request.GreetingDto.GuildID, cancellationToken);
            if (guildConfig is null)
                return Result<GuildGreetingDTO>.FromError(new NotFoundError("Guild config not found"));

            var existingGreeting = guildConfig.Greetings
                                              .FirstOrDefault(g => g.Id == request.GreetingDto.Id);
            if (existingGreeting is not null)
                return Result<GuildGreetingDTO>.FromError(new GenericError("Greeting already exists"));

            // If no greeting exists, create a new one but make sure the ID is not set
            var newGreeting = (request.GreetingDto with { Id = 0 }).Adapt<GuildGreetingEntity>();

            guildConfig.Greetings.Add(newGreeting);
            var saved = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            return saved 
                ? Result<GuildGreetingDTO>.FromSuccess(newGreeting.Adapt<GuildGreetingDTO>())
                : Result<GuildGreetingDTO>.FromError(new GenericError("Failed to save greeting"));
        }
    }
}