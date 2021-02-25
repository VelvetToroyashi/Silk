using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Data.Models;

namespace Silk.Data.MediatR.Handlers
{
    public class UserHandler
    {
        public class GetHandler : IRequestHandler<UserRequest.Get, User?>
        {
            private readonly SilkDbContext _db;
            public GetHandler(SilkDbContext db) => _db = db;

            public async Task<User?> Handle(UserRequest.Get request, CancellationToken cancellationToken)
            {
                User? user = await _db.Users
                    .Include(u => u.Infractions)
                    .FirstOrDefaultAsync(u => u.Id == request.UserId && 
                                                u.GuildId == request.GuildId, cancellationToken);
                return user;
            }
        }
        
        public class AddHandler : IRequestHandler<UserRequest.Add, User>
        {
            private readonly SilkDbContext _db;

            public AddHandler(SilkDbContext db) => _db = db;

            public async Task<User> Handle(UserRequest.Add request, CancellationToken cancellationToken)
            {
                var user = new User {Id = request.UserId, GuildId = request.GuildId, Flags = request.Flags ?? UserFlag.None};
                _db.Users.Add(user);
                await _db.SaveChangesAsync(cancellationToken);
                return user;
            }
        }

        public class UpdateHandler : IRequestHandler<UserRequest.Update, User>
        {
            private readonly SilkDbContext _db;

            public UpdateHandler(SilkDbContext db)
            {
                _db = db;
            }
            
            public async Task<User> Handle(UserRequest.Update request, CancellationToken cancellationToken)
            {
                User user = await _db.Users.FirstAsync(u => u.Id == request.UserId && u.GuildId == request.GuildId, cancellationToken);
                
                user.Flags = request.Flags ?? user.Flags;
                user.Infractions = request.Infractions ?? user.Infractions;
                await _db.SaveChangesAsync(cancellationToken);
                
                return user;
            }
        }

        public class GetOrCreateHandler : IRequestHandler<UserRequest.GetOrCreate, User>
        {
            private readonly SilkDbContext _db;
            public GetOrCreateHandler(SilkDbContext db) => _db = db;

            public async Task<User> Handle(UserRequest.GetOrCreate request, CancellationToken cancellationToken)
            {
                User? user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId && u.GuildId == request.GuildId, cancellationToken);
                if (user is null)
                {
                    user = new()
                    {
                        GuildId = request.GuildId,
                        Id = request.UserId,
                        Flags = request.Flags ?? UserFlag.None,
                        Infractions = request.Infractions ?? new()
                    };
                }
                return user;
            }
        }
    }
}