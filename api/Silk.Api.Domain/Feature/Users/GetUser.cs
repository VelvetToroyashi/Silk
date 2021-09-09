using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Api.Data;
using Silk.Api.Data.Entities;

namespace Silk.Api.Domain.Feature.Users
{
	public static class GetUser
	{
		public sealed record Request(string Id) : IRequest<ApiUser>;
		
		public sealed class Handler : IRequestHandler<Request, ApiUser>
		{
			private readonly ApiContext _db;
			public Handler(ApiContext db) => _db = db;
			
			public async Task<ApiUser> Handle(Request request, CancellationToken cancellationToken)
			{
				var user = await _db.Users.FirstOrDefaultAsync(u => u.DiscordId == request.Id, cancellationToken);

				return user;
			}
		}
	}
}