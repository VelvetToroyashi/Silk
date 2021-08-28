using System.Text;
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
		public sealed record Request(string UserName, string Password, string Salt, string ApiKey) : IRequest<ApiUser>;
		
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
				var passSalt = Encoding.UTF8.GetBytes(request.Salt);
				var pass = _crypto.HashPassword(request.Password, passSalt);
			
				var user = new ApiUser
				{
					Key = new ApiKey() {KeyHash = request.ApiKey},
					Username = request.UserName,
					PasswordHash = Encoding.UTF8.GetString(pass),
					PasswordSalt = request.Salt
				};
				
				_db.Users.Add(user);
				await _db.SaveChangesAsync(cancellationToken);

				
				
				return user;
			}
		}
	}
}