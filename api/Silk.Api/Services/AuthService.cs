using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Silk.Api.Domain.Feature.Users;

namespace Silk.Api.Services
{
	public sealed class AuthService : IAuthorizationHandler
	{
		private readonly IMediator _mediator;
		public AuthService(IMediator mediator)
		{
			_mediator = mediator;
		}
		
		public async Task HandleAsync(AuthorizationHandlerContext context)
		{
			var id = context.User.Claims.FirstOrDefault(c => c.Type == "ist")?.Value;

			if (id is null || !ulong.TryParse(id, out _)) 
			{
				context.Fail();
				return;
			}

			var user = await _mediator.Send(new GetUser.Request(id));

			if (user is null)
			{
				context.Fail();
				return;
			}

			var genAtString = context.User.Claims.FirstOrDefault(c => c.Type == "iat")?.Value;

			if (genAtString is null || !DateTime.TryParse(genAtString, out var generatedAt))
			{
				context.Fail();
				return;
			}
			
			if (generatedAt < user.ApiKeyGenerationTimestamp) // TODO: Validate generation timestamp isn't in the future, either. //
				context.Fail();
			
		}
	}
}