using System;
using System.Threading;
using System.Threading.Tasks;
using Isopoh.Cryptography.Argon2;
using MediatR;
using Silk.Api.Data;
using Silk.Api.Data.Entities;

namespace Silk.Api.Domain.Feature.Users
{
	public static class AddUser
	{
		public sealed record Request(string UserName, string Password, string Salt) : IRequest<User>;
		
		public sealed class Handler : IRequestHandler<Request, User>
		{
			private readonly ApiContext _db;
			public Handler(ApiContext db) => _db = db;

			public async Task<User> Handle(Request request, CancellationToken cancellationToken)
			{
				var user = new User
				{
					Key = Guid.NewGuid(),
					Username = request.UserName,
					Password = Argon2.Hash(request.Password, request.Salt)
				};
				
				_db.Users.Add(user);
				await _db.SaveChangesAsync(cancellationToken);

				return user;
			}
		}
	}
}