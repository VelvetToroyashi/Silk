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
            if (existingGreeting is null)
                return Result<GuildGreetingDTO>.FromError(new NotFoundError("Greeting does not exist"));

            var updatedGreetingEntity = request.GreetingDto.Adapt(existingGreeting);
            var saved = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            return saved 
                ? Result<GuildGreetingDTO>.FromSuccess(updatedGreetingEntity.Adapt<GuildGreetingDTO>())
                : Result<GuildGreetingDTO>.FromError(new GenericError("Failed to update greeting"));
        }
    }
}