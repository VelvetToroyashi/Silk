using System;
using Microsoft.AspNetCore.Identity;
using Silk.Api.Data.Entities;

namespace Silk.Api.Services
{
	public class AuthenticationService
	{
		private readonly UserManager<ApiUser> _users;
		public AuthenticationService(UserManager<ApiUser> users)
		{
			Console.WriteLine("In ctor");
			_users = users;
			_users.CreateAsync(new(), "password").GetAwaiter().GetResult();
			Console.WriteLine("Done");
		}
		
		
	}
}