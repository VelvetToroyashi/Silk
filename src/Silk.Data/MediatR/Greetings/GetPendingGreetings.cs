using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Greetings;

public static class GetPendingGreetings
{
    public record Request() : IRequest<IReadOnlyList<PendingGreetingEntity>>;
    
    internal class Handler : IRequestHandler<Request, IReadOnlyList<PendingGreetingEntity>>
    {
        private readonly GuildContext _db;

        public Handler(GuildContext db) => _db = db;

        public async Task<IReadOnlyList<PendingGreetingEntity>> Handle(Request request, CancellationToken cancellationToken)
            => await _db.PendingGreetings.ToArrayAsync(cancellationToken);
    }

}