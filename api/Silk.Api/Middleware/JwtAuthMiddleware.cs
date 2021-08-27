using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Isopoh.Cryptography.Argon2;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Silk.Api.Data.Entities;
using Silk.Api.Domain.Feature.Users;
using Silk.Api.Helpers;

namespace Silk.Api
{
	public sealed class JwtAuthMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IMediator _mediator;
		private readonly ApiSettings _appSettings;

		public JwtAuthMiddleware(RequestDelegate next, IOptions<ApiSettings> settings, IMediator mediator)
		{
			_next = next;
			_mediator = mediator;
			_appSettings = settings.Value;
		}

		public async Task Invoke(HttpContext context)
		{
			var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

			if (token != null)
				await AttachUserToContext(context, token);

			await _next(context);
		}

		private async Task AttachUserToContext(HttpContext context, string token)
		{
			try
			{
				var tokenHandler = new JwtSecurityTokenHandler();
				var key = Encoding.ASCII.GetBytes(_appSettings.JwtSecret);
				tokenHandler.ValidateToken(token, new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidateIssuer = false,
					ValidateAudience = false,
					// set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
					ClockSkew = TimeSpan.Zero
				}, out SecurityToken validatedToken);

				var jwtToken = (JwtSecurityToken)validatedToken;
				var userKey = jwtToken.Claims.FirstOrDefault(c => c.Type == "key")?.Value;
				var userName = jwtToken.Claims.First(c => string.Equals(nameof(User.Username), c.Type, StringComparison.OrdinalIgnoreCase)).Value;
				var userPass = jwtToken.Claims.First(c => string.Equals(nameof(User.Password), c.Type, StringComparison.OrdinalIgnoreCase)).Value;
				
				context.Items["user"] = null;
				
				if (userKey is null)
					return;

				var parsedKey = Guid.Parse(userKey);

				var user = await _mediator.Send(new GetUser.Request(parsedKey));
				
				if (user.Key != parsedKey || user.Username != userName || !Argon2.Verify(user.Password, userPass, _appSettings.HashSalt))
					return; 
				
				context.Items["user"] = user;
			}
			catch { /* Do nothing */ }
		}
	}
}