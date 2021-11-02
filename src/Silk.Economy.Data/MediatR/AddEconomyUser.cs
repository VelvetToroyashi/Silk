using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Economy.Data.Models;

namespace Silk.Economy.Data
{
	public static class AddEconomyUser
	{
		//create a request record that implements IRequest<EconomyUser> for MediatR
		public record Request(ulong UserId) : IRequest<EconomyUser>;
		
		//create a handler that implements IRequestHandler<Request, EconomyUser> for MediatR
		public class Handler : IRequestHandler<Request, EconomyUser>
		{
			private readonly EconomyContext _db;
			public Handler(EconomyContext db) => _db = db;

			public async Task<EconomyUser> Handle(Request request, CancellationToken cancellationToken)
            {
                EconomyUser user = await _db.EconomyUsers.FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);
                
                if (user == null)
                {
	                user = new() { UserId = request.UserId };
                    _db.EconomyUsers.Add(user);
                }

                await _db.SaveChangesAsync(cancellationToken);
                return user;
            }
        }
	}
}