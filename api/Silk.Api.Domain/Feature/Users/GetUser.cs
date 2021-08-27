using System;
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
		public sealed record Request(Guid Key) : IRequest<User>;
		
		
		public sealed class Handler : IRequestHandler<Request, User>
		{
			private readonly ApiContext _db;
			public Handler(ApiContext db) => _db = db;
			
			public async Task<User> Handle(Request request, CancellationToken cancellationToken)
			{
				var user = await _db.Users.FirstOrDefaultAsync(d => d.Key == request.Key, cancellationToken);
				
				return user;
			}
		}
	}
}