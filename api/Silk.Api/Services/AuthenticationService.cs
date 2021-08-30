using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Silk.Api.Data.Entities;
using Silk.Api.Models;

namespace Silk.Api.Services
{
	public class AuthenticationService : IAuthorizationHandler
	{
		private readonly UserManager<ApiUser> _users;
		private readonly RoleManager<ApiUserRole> _roles;
		public AuthenticationService(UserManager<ApiUser> users, RoleManager<ApiUserRole> roles)
		{
			_users = users;
			_roles = roles;
		}

		public async Task HandleAsync(AuthorizationHandlerContext context)
		{
			
			Console.WriteLine(context.User);
		}
	}
}