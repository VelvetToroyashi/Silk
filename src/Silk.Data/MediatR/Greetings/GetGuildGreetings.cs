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
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public async Task<List<GuildGreeting>> Handle(Request request, CancellationToken cancellationToken)
        {
            return await _db.GuildGreetings
                            .AsNoTracking()
                            .Where(g => g.GuildID == request.GuildID)
                            .ProjectToType<GuildGreeting>()
                            .ToListAsync(cancellationToken: cancellationToken);
        }
    }
}