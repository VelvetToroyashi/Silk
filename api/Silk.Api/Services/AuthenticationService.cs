using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Silk.Api.Services
{
	public class AuthenticationService : IAuthorizationHandler
	{
		public async Task HandleAsync(AuthorizationHandlerContext context)
		{
			
			Console.WriteLine(context.User);
		}
	}
}