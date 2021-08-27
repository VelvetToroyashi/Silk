using System;
using System.Collections.Generic;
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
using Silk.Api.Domain.Services;
using Silk.Api.Helpers;

namespace Silk.Api.Controllers
{
	[ApiController]
	
	[Route("api/v1/[controller]")]
	public class AuthenticationController : ControllerBase
	{
		private readonly IMediator _mediator;
		private readonly ApiSettings _settings;
		private readonly CryptoHelper _crypto;
		public AuthenticationController(IMediator mediator, IOptions<ApiSettings> settings, CryptoHelper crypto)
		{
			_mediator = mediator;
			_crypto = crypto;
			_settings = settings.Value;
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> Authenticate(AddUser.Request request)
		{
			var user = await _mediator.Send(new AddUser.Request(request.UserName, request.Password, Encoding.UTF8.GetString(_crypto.CreateSalt())));
			
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.JwtSecret));    
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			var claims = new List<Claim>();
			claims.Add(new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())); // Token ID //
			claims.Add(new("usr", user.Username));
			claims.Add(new("psw", user.Password));
			claims.Add(new("key", user.Key.ToString()));
			
			var token = new JwtSecurityToken("velvetthepanda.dev", //Issuer
				"velvetthepanda.dev",  //Audience    
				claims,
				expires: DateTime.Now.AddYears(100),
				signingCredentials: credentials);    
			var jwt_token = new JwtSecurityTokenHandler().WriteToken(token);  
			
			return Ok(new { token = jwt_token});
		}

		[HttpDelete]
		[Helpers.Authorize]
		public async Task<IActionResult> RevokeAccount()
		{


			return NoContent();
		}
	}
}