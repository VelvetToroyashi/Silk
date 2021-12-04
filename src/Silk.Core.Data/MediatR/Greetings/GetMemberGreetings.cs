using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR
{
    public static class GetMemberGreetings
    {
        public record Request : IRequest<IEnumerable<MemberGreetingEntity>>;
        
        internal class Handler : IRequestHandler<Request, IEnumerable<MemberGreetingEntity>>
        {
            private readonly GuildContext _db;
            public Handler(GuildContext db) => _db = db;

            public async Task<IEnumerable<MemberGreetingEntity>> Handle(Request request, CancellationToken cancellationToken)
                => await _db.MemberGreetings.ToListAsync(cancellationToken);
        }
    }
}