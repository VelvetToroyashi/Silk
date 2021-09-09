using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Api.Data;

namespace Silk.Api.Domain.Feature.Users
{
	public static class EditUser
	{
		public sealed record Request(string DiscordId) : IRequest<bool>;
		
		public sealed class Handler : IRequestHandler<Request, bool>
		{
			private readonly ApiContext _db;
			public Handler(ApiContext db) => _db = db;
			
			public async Task<bool> Handle(Request request, CancellationToken cancellationToken)
			{
				var user = await _db.Users.Include(u => u.ApiKey).SingleOrDefaultAsync(u => u.DiscordId == request.DiscordId, cancellationToken);

				if (user is null)
					return false;

				var now = DateTime.Now;

				user.ApiKey.GeneratedAt = now;
				user.ApiKeyGenerationTimestamp = now;

				await _db.SaveChangesAsync(cancellationToken);
				return true;
			}
		}
	}
}