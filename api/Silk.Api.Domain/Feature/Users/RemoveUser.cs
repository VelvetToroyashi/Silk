using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Api.Data;

namespace Silk.Api.Domain.Feature.Users
{
	public static class RemoveUser
	{
		public sealed record Request(string UserKey) : IRequest<bool>;
		
		public sealed class Handler : IRequestHandler<Request, bool>
		{
			private readonly ApiContext _db;
			public Handler(ApiContext db) => _db = db;
			
			public async Task<bool> Handle(Request request, CancellationToken cancellationToken)
			{
				var user = await _db.Users
					.Include(u => u.ApiKey)
					.Include(u => u.Infractions)
					.SingleOrDefaultAsync(u => u.DiscordId == request.UserKey, cancellationToken);
				
				if (user is null)
					return false;

				_db.Users.Remove(user);
				_db.Keys.Remove(user.ApiKey);
				_db.Infractions.RemoveRange(user.Infractions);

				await _db.SaveChangesAsync(cancellationToken);

				return true;
			}
		}
	}
}