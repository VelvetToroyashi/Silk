using System;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
			var apiKeyGUID = Guid.NewGuid().ToString();
			var apiKeyBytes = _crypto.HashPassword(apiKeyGUID, Encoding.UTF8.GetBytes(_settings.HashSalt));
			var apiKeyString = Encoding.UTF8.GetString(apiKeyBytes);
			
			request = new(request.UserName, request.Password, Encoding.UTF8.GetString(_crypto.CreateSalt()), apiKeyString);
			var user = await _mediator.Send(request);
			
			return Ok(new { token = apiKeyGUID});
		}

		[Authorize]
		[HttpDelete]
		public async Task<IActionResult> RevokeAccount()
		{

			return NoContent();
		}
	}
}