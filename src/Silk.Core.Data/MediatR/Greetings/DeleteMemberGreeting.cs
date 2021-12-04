using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Results;

namespace Silk.Core.Data.MediatR
{
    public static class DeleteMemberGreeting
    {
        public record Request(ulong GuildId, ulong UserId) : IRequest<Result>;
        
        internal class Handler : IRequestHandler<Request, Result>
        {
            private readonly GuildContext _db;
            public Handler(GuildContext db) => _db = db;

            public async Task<Result> Handle(Request request, CancellationToken cancellationToken)
            {
                var existing = await _db
                                    .MemberGreetings
                                    .FirstOrDefaultAsync(g => g.GuildId == request.GuildId && g.UserId == request.UserId, cancellationToken);
                
                if (existing == null)
                    return Result.FromError(new NotFoundError("No greeting exists for the specified member in the specified guild."));
                
                _db.MemberGreetings.Remove(existing);
                await _db.SaveChangesAsync(cancellationToken);
                
                return Result.FromSuccess();
            }
        }
    }
}