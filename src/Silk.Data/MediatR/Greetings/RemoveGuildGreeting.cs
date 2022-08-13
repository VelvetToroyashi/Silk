using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Rest.Core;
using Remora.Results;

namespace Silk.Data.MediatR.Greetings;

public static class RemoveGuildGreeting
{
    public record Request(int GreetingId, Snowflake GuildId) : IRequest<Result>;
    
    internal class Handler : IRequestHandler<Request, Result>
    {
        private readonly ILogger<Handler>                _logger;
        private readonly GuildContext _db;

        public Handler
        (
            ILogger<Handler>                logger,
            GuildContext db
        )
        {
            _logger           = logger;
            _db = db;
        }

        public async Task<Result> Handle(Request request, CancellationToken cancellationToken)
        {
            var guildConfig = await _db.GuildConfigs
                                       .Include(gc => gc.Greetings)
                                       .FirstOrDefaultAsync(gc => gc.GuildID == request.GuildId, cancellationToken);
            if (guildConfig is null)
                return Result.FromError(new NotFoundError("Guild config not found"));
            
            var greeting = guildConfig.Greetings
                                      .FirstOrDefault(g => g.Id == request.GreetingId);
            if (greeting is null)
                return Result.FromError(new NotFoundError("Greeting not found"));

            bool removed = false;

            try
            {
                guildConfig.Greetings.Remove(greeting);
                removed = await _db.SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception e)
            {
                _logger.LogError("Error removing greeting - {ExceptionMessage}", e.Message);
            }

            return removed 
                ? Result.FromSuccess() 
                : Result.FromError(new GenericError("Failed to remove greeting"));
        }
    }
}