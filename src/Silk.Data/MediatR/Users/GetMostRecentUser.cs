using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Remora.Rest.Core;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.Users;

public static class GetMostRecentUser
{
    public record Request(Snowflake GuildID) : IRequest<UserEntity>;
    
    internal class Handler : IRequestHandler<Request, UserEntity>
    {
        private readonly GuildContext _db;
        public Handler(GuildContext db) => _db = db;

        public async Task<UserEntity> Handle(Request request, CancellationToken cancellationToken)
        {
            var guild = await _db.Guilds
                           .Include(g => g.Users)
                                 .ThenInclude(u => u.History)
                           .FirstAsync(g => g.ID == request.GuildID, cancellationToken);

            var user = guild.Users.OrderByDescending(u => u.History).First();

            return user;
        }
    }
}