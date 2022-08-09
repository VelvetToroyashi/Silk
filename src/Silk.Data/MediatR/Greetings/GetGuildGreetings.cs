using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.DTOs.Guilds.Config;

namespace Silk.Data.MediatR.Greetings;

public static class GetGuildGreetings
{
    public record Request(Snowflake GuildID) : IRequest<List<GuildGreeting>>;

    internal class Handler : IRequestHandler<Request, List<GuildGreeting>>
    {
        private readonly IDbContextFactory<GuildContext> _dbContextFactory;

        public Handler(IDbContextFactory<GuildContext> dbContextFactory)
            => _dbContextFactory = dbContextFactory;

        public async Task<List<GuildGreeting>> Handle(Request request, CancellationToken cancellationToken)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await context.GuildGreetings
                                .AsNoTracking()
                                .Where(g => g.GuildID == request.GuildID)
                                .ProjectToType<GuildGreeting>()
                                .ToListAsync(cancellationToken: cancellationToken);
        }
    }
}