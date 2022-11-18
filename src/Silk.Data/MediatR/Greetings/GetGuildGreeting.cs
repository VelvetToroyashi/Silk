using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Remora.Results;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Greetings;

public static class GetGuildGreeting
{
    public record Request(int ID) : IRequest<Result<GuildGreetingEntity>>;
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class Handler : IRequestHandler<Request, Result<GuildGreetingEntity>>
    {
        private readonly GuildContext _db;
        
        public Handler(GuildContext db) => _db = db;

        public async ValueTask<Result<GuildGreetingEntity>> Handle(Request request, CancellationToken cancellationToken)
        {
            
            
            var result = await _db.GuildGreetings.FirstOrDefaultAsync(gg => gg.Id == request.ID, cancellationToken);
            
            return result is not null
                ? Result<GuildGreetingEntity>.FromSuccess(result) 
                : Result<GuildGreetingEntity>.FromError(new NotFoundError());
        }
    }

}