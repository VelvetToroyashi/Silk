using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Api.Data;
using Silk.Api.Data.Entities;

namespace Silk.Api.Domain.Feature.Users
{
	public static class AddUser
	{
		public sealed record Request(string Id, string ApiKey) : IRequest<ApiUser>;
		
		public sealed class Handler : IRequestHandler<Request, ApiUser>
		{
			private readonly ApiContext _db;
			public Handler(ApiContext db) => _db = db;

			public async Task<ApiUser> Handle(Request request, CancellationToken cancellationToken)
			{
				var user = new ApiUser
				{
					ApiKey = new()
					{
						GeneratedAt = DateTime.Now,
						DiscordUserId = request.Id
					},
					DiscordId = request.Id,
					ApiKeyGenerationTimestamp = DateTime.Now,
					
				};
				
				_db.Users.Add(user);
				await _db.SaveChangesAsync(cancellationToken);
				
				return user;
			}
		}
	}
}