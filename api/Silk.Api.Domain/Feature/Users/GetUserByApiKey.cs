using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Api.Data;
using Silk.Api.Data.Entities;

namespace Silk.Api.Domain.Feature.Users
{
	public static class GetUserByApiKey
	{
		public sealed record Request(string Key) : IRequest<ApiUser>;
		
		public sealed class Handler : IRequestHandler<Request, ApiUser>
		{
			private readonly ApiContext _db;
			public Handler(ApiContext db) => _db = db;
			
			public async Task<ApiUser> Handle(Request request, CancellationToken cancellationToken)
			{
				var user = await _db.Users
					.Include(u => u.Key)
					.FirstOrDefaultAsync(d => d.Key.KeyHash == request.Key, cancellationToken);
				
				return user;
			}
		}
	}
}