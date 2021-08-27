using System;
using System.Threading.Tasks;
using MediatR;
using Silk.Api.Data.Entities;
using Silk.Api.Domain.Feature.Users;
using Silk.Api.Domain.Services;

namespace Silk.Api.Services
{
	public sealed class UserService : IUserService
	{
		private readonly IMediator _mediator;
		public UserService(IMediator mediator) => _mediator = mediator;
		
		public Task<User> GetUserByKey(Guid key) 
			=> _mediator.Send(new GetUser.Request(key));
	}
}