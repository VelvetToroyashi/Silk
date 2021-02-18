using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Database.Models;

namespace Silk.Core.Database.MediatR.Handlers
{
    public class UserHandler
    {
        public class UserGetRequestHandler : IRequestHandler<UserRequest.GetUserRequest, User?>
        {
            private readonly SilkDbContext _db;

            public UserGetRequestHandler(SilkDbContext db)
            {
                _db = db;
            }
        
            public async Task<User?> Handle(UserRequest.GetUserRequest request, CancellationToken cancellationToken)
            {
                User? user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId && u.GuildId == request.GuildId, cancellationToken);
                return user;
            }
        }
        
        public class UserAddRequestHandler : IRequestHandler<UserRequest.AddUserRequest, User>
        {
            private readonly SilkDbContext _db;

            public UserAddRequestHandler(SilkDbContext db)
            {
                _db = db;
            }

            public async Task<User> Handle(UserRequest.AddUserRequest request, CancellationToken cancellationToken)
            {
                var user = new User {Id = request.UserId, GuildId = request.GuildId, Flags = request.Flags ?? UserFlag.None};
                _db.Attach(user);
                await _db.SaveChangesAsync(cancellationToken);
                return user;
            }
        }
    }
}