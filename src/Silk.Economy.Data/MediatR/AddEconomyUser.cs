using MediatR;
using Silk.Economy.Data.Models;

namespace Silk.Economy.Data
{
	public static class AddEconomyUser
	{
		//create a request record that implements IRequest<EconomyUser> for MediatR
		public record Request(ulong UserId) : IRequest<EconomyUser>;
		
		//create a handler that implements IRequestHandler<Request, EconomyUser> for MediatR
		

	}
}