using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Greetings;

public static class RemoveGuildGreeting
{
    public record Request(int GreetingId, Snowflake GuildId) : IRequest<Result>;
    
    internal class Handler : IRequestHandler<Request, Result>
    {
        private readonly IDbContextFactory<GuildContext> _dbContextFactory;

        public Handler(IDbContextFactory<GuildContext> dbContextFactory) 
            => _dbContextFactory = dbContextFactory;

        public async Task<Result> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var guildConfig = await dbContext.GuildConfigs
                                             .Include(gc => gc.Greetings)
                                             .FirstOrDefaultAsync(gc => gc.GuildID == request.GuildId, cancellationToken);
            if (guildConfig is null)
                return Result.FromError(new NotFoundError("Guild config not found"));
            
            var greeting = guildConfig.Greetings
                                      .FirstOrDefault(g => g.Id == request.GreetingId);
            if (greeting is null)
                return Result.FromError(new NotFoundError("Greeting not found"));
            
            guildConfig.Greetings.Remove(greeting);
            var removed = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            return removed 
                ? Result.FromSuccess() 
                : Result.FromError(new GenericError("Failed to remove greeting"));
        }
    }
}