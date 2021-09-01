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
		private static SigningCredentials _signingCreds;

		public AuthenticationController(IMediator mediator, IOptions<ApiSettings> settings, JwtSecurityTokenHandler handler, DiscordOAuthService oauth)
		{
			_mediator = mediator;
			_handler = handler;
			_oauth = oauth;
			_settings = settings.Value;

			_signingCreds ??= new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.JwtSecret)), SecurityAlgorithms.HmacSha256);
		}

		/// <summary> Registers an application with the API. Body returns the API token. </summary>
		/// <response code="409">The specified application was already registered.</response>
		/// <response code="201">The application was successfully registered.</response>
		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> Authenticate([FromBody] ApplicationOAuthModel auth)
		{
			var res = await _oauth.VerifyDiscordApplicationAsync(auth.Id.ToString(), auth.Secret);

			if (!res.Authenticated)
				return BadRequest(new { message = "An invalid id or client secret was provided, and a bearer token could not be generated." });

			var user = await _mediator.Send(new GetUser.Request(res.Id.ToString()));

			if (user is not null)
				return Conflict(new { message = "An application with that id was already registered." });

			var token = new JwtSecurityToken(_settings.JwtSigner, claims:
				new Claim[]
				{
					new("ist", res.Id.ToString(CultureInfo.InvariantCulture)),
					new("iat", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture))
				}, signingCredentials: _signingCreds);

			var apiToken = _handler.WriteToken(token);
			var req = new AddUser.Request(res.Id.ToString(), apiToken);
			await _mediator.Send(req);

			return Created(nameof(Authenticate), new { token = apiToken });
		}


		/// <summary> (NOT CURRENTLY IMPLEMENTED) Deletes a registed application. </summary>
		/// <response code="501">Told you.</response>
		[Authorize]
		[HttpDelete]
		public async Task<IActionResult> RevokeAccount()
		{
			// TODO: Delete account here // 

			return StatusCode(501, new { Message = "This endpoint has yet to be implemented. Try again later." });
		}

		/// <summary>Regenerates a token for the specified application.</summary>
		/// <response code="200">The token was successfully regenerated.</response>
		[HttpPatch]
		[AllowAnonymous]
		[Route("token")]
		public async Task<IActionResult> RevokeToken([FromBody] ApplicationOAuthModel auth)
		{
			var res = await _oauth.VerifyDiscordApplicationAsync(auth.Id.ToString(), auth.Secret);

			if (!res.Authenticated)
				return BadRequest(new { message = "An invalid id or client secret was provided, and a bearer token could not be generated." });

			var token = new JwtSecurityToken(_settings.JwtSigner, claims:
				new Claim[]
				{
					new("ist", res.Id.ToString(CultureInfo.InvariantCulture)),
					new("iat", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture))
				}, signingCredentials: _signingCreds);

			var apiToken = _handler.WriteToken(token);

			var req = new EditUser.Request(res.Id.ToString());
			await _mediator.Send(req);

			return Ok(new { token = apiToken });
		}
	}
}