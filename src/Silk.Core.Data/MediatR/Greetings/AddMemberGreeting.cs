using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Results;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data.MediatR
{
    public static class AddMemberGreeting
    {
        public record Request(ulong GuildId, ulong UserId, GreetingOption Option) : IRequest<Result<MemberGreetingEntity>>;
        
        internal class Handler : IRequestHandler<Request, Result<MemberGreetingEntity>>
        {
            private readonly GuildContext _db;
            public Handler(GuildContext db) => _db = db;

            public async Task<Result<MemberGreetingEntity>> Handle(Request request, CancellationToken cancellationToken)
            {
                var existing = await _db
                                    .MemberGreetings
                                    .FirstOrDefaultAsync(mge => mge.GuildId == request.GuildId && mge.UserId == request.UserId, cancellationToken);

                if (existing is not null)
                    return 
                        Result<MemberGreetingEntity>
                       .FromError(new InvalidOperationError("Member already has a greeting record in the specified guild."));

                existing = new()
                {
                    UserId = request.UserId,
                    GuildId = request.GuildId,
                    Greeting = request.Option
                };

                _db.Add((object)existing);
                await _db.SaveChangesAsync(cancellationToken);
                
                return Result<MemberGreetingEntity>.FromSuccess(existing);
            }
        }
    }
}