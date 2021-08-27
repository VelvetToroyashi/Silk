using System;
using System.Text;
using System.Text.Json.Serialization;
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
		public sealed record Request(string UserName, string Password, [property: JsonIgnore] string Salt) : IRequest<User>;
		
		public sealed class Handler : IRequestHandler<Request, User>
		{
			private readonly ApiContext _db;
			private readonly CryptoHelper _crypto;
			public Handler(ApiContext db, CryptoHelper crypto)
			{
				_db = db;
				_crypto = crypto;
			}

			public async Task<User> Handle(Request request, CancellationToken cancellationToken)
			{
				var pass = _crypto.HashPassword(request.Password, Encoding.UTF8.GetBytes(request.Salt));
				
				var user = new User
				{
					Key = Guid.NewGuid(),
					Username = request.UserName,
					Password = Encoding.UTF8.GetString(pass),
					PasswordSalt = request.Salt
				};
				
				_db.Users.Add(user);
				await _db.SaveChangesAsync(cancellationToken);

				return user;
			}
		}
	}
}