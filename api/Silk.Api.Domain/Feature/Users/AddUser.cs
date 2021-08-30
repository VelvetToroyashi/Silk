using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Api.Data;
using Silk.Api.Data.Entities;
using Silk.Api.Domain.Services;

namespace Silk.Api.Domain.Feature.Users
{
	public static class AddUser
	{
		public sealed record Request(string Id, string ApiKey) : IRequest<ApiUser>;
		
		public sealed class Handler : IRequestHandler<Request, ApiUser>
		{
			private readonly ApiContext _db;
			private readonly CryptoHelper _crypto;
			public Handler(ApiContext db, CryptoHelper crypto)
			{
				_db = db;
				_crypto = crypto;
			}

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