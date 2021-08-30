using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Silk.Api.Domain.Feature.Users;
using Silk.Api.Helpers;
using Silk.Api.Models;
using Silk.Api.Services;

namespace Silk.Api.Controllers
{
	[ApiController]
	[Route("api/v1/[controller]")]
	public class AuthenticationController : ControllerBase
	{
		private readonly JwtSecurityTokenHandler _handler;
		private readonly IMediator _mediator;
		private readonly DiscordOAuthService _oauth;
		private readonly ApiSettings _settings;
		public AuthenticationController(IMediator mediator, IOptions<ApiSettings> settings, JwtSecurityTokenHandler handler, DiscordOAuthService oauth)
		{
			_mediator = mediator;
			_handler = handler;
			_oauth = oauth;
			_settings = settings.Value;
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> Authenticate([FromBody] ApplicationOAuthModel auth)
		{
			var res = await _oauth.VerifyDiscordApplicationAsync(auth.Id.ToString(), auth.Secret);

			if (!res.Authenticated)
				return BadRequest(new { message = "An invalid id or client secret was provided, and a bearer token could not be generated." });

			var token = new JwtSecurityToken(_settings.JwtSigner, 
				claims: new Claim[]
				{
					new ("iat", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
					new("ist", res.Id.ToString(CultureInfo.InvariantCulture))
				},
				signingCredentials: new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.JwtSecret)), SecurityAlgorithms.HmacSha256));


			var apiToken = _handler.WriteToken(token);
			var req = new AddUser.Request(res.Id.ToString(), apiToken);
			
			return Ok(new { token = apiToken});
		}
		
		
		[Authorize]
		[HttpDelete]
		public async Task<IActionResult> RevokeAccount()
		{

			return NoContent();
		}
	}
}