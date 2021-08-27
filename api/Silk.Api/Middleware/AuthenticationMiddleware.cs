using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Silk.Api.Domain.Feature.Users;
using Silk.Api.Domain.Services;
using Silk.Api.Helpers;

namespace Silk.Api
{
	public sealed class AuthenticationMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IMediator _mediator;
		private readonly ApiSettings _appSettings;
		private readonly CryptoHelper _crypto;

		public AuthenticationMiddleware(RequestDelegate next, IOptions<ApiSettings> settings, IMediator mediator, CryptoHelper crypto)
		{
			_next = next;
			_mediator = mediator;
			_crypto = crypto;
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
			context.Items["user"] = null;

			var user = await _mediator.Send(new GetUserByApiKey.Request(token));

			if (user is not null)
				context.Items["user"] = user;
		}
	}
}