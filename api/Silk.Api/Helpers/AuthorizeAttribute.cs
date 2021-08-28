using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Silk.Api.Data.Entities;

namespace Silk.Api.Helpers
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class AuthorizeAttribute : Attribute, IAuthorizationFilter
	{
		public void OnAuthorization(AuthorizationFilterContext context)
		{
			var user = (ApiUser)context.HttpContext.Items["User"];
			
			if (user is null || user.Key.Revoked)
				context.Result = new UnauthorizedResult();
		}
	}
}