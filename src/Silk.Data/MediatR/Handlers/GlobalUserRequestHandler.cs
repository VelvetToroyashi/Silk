using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Data.Models;

namespace Silk.Data.MediatR.Handlers
{
    public class GlobalUserRequestHandler
    {
        public class GetHandler : IRequestHandler<GlobalUserRequest.Get, GlobalUser>
        {
            private readonly SilkDbContext _db;
            public GetHandler(SilkDbContext db)
            {
                _db = db;
            }

            public async Task<GlobalUser> Handle(GlobalUserRequest.Get request, CancellationToken cancellationToken)
            {
                GlobalUser? user = await _db.GlobalUsers.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
                return user;
            }
        }

        public class AddHandler : IRequestHandler<GlobalUserRequest.Add, GlobalUser>
        {
            private readonly SilkDbContext _db;
            public AddHandler(SilkDbContext db)
            {
                _db = db;
            }
            public async Task<GlobalUser> Handle(GlobalUserRequest.Add request, CancellationToken cancellationToken)
            {
                GlobalUser user = new()
                {
                    Id = request.UserId,
                    Cash = 0,
                    LastCashOut = DateTime.MinValue
                };
                _db.GlobalUsers.Add(user);
                await _db.SaveChangesAsync(cancellationToken);
                return user;
            }
        }

        public class UpdateHandler : IRequestHandler<GlobalUserRequest.Update, GlobalUser>
        {
            private readonly SilkDbContext _db;
            public UpdateHandler(SilkDbContext db)
            {
                _db = db;
            }
            public async Task<GlobalUser> Handle(GlobalUserRequest.Update request, CancellationToken cancellationToken)
            {
                GlobalUser user = await _db.GlobalUsers.FirstAsync(u => u.Id == request.UserId, cancellationToken);
                user.Cash = request.Cash;
                user.LastCashOut = request.LastCashOut;
                await _db.SaveChangesAsync(cancellationToken);
                return user;
            }
        }

        public class GetOrCreateHandler : IRequestHandler<GlobalUserRequest.GetOrCreate, GlobalUser>
        {
            private readonly SilkDbContext _db;
            public GetOrCreateHandler(SilkDbContext db)
            {
                _db = db;
            }
            // TODO: Make this proper
            public async Task<GlobalUser> Handle(GlobalUserRequest.GetOrCreate request, CancellationToken cancellationToken)
            {
                GlobalUser? user = await _db.GlobalUsers.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
                user ??= new()
                {
                    Id = request.UserId,
                    LastCashOut = DateTime.MinValue
                };
                
                return user;
            }
        }
    }
}