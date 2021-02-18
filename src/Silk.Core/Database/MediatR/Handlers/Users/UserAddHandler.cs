using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Database.Models;

namespace Silk.Core.Database.MediatR.Handlers.Users
{
    public class UserAddHandler : IRequestHandler<AddUserRequest, User>
    {
        private readonly SilkDbContext _db;

        public UserAddHandler(SilkDbContext db)
        {
            _db = db;
        }
            
        public async Task<User> Handle(AddUserRequest request, CancellationToken cancellationToken)
        {
            var user = new User { Id = request.UserId, GuildId = request.GuildId, Flags = request.Flags ?? UserFlag.None };
            _db.Attach(user);
            
            await _db.SaveChangesAsync(cancellationToken);
            return user;
        }
    }
}