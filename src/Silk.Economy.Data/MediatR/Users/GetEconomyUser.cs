using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Economy.Data.Models;

namespace Silk.Economy.Data
{
	public static class GetEconomyUser
	{
		public sealed record Request(ulong UserId) : IRequest<EconomyUser>;
		
		public sealed class Handler : IRequestHandler<Request, EconomyUser>
        {
            private readonly EconomyContext _db;
            private readonly IMediator _mediator;

            public Handler(EconomyContext db, IMediator mediator)
            {
	            _db = db;
	            _mediator = mediator;
            }

            public async Task<EconomyUser> Handle(Request request, CancellationToken cancellationToken)
            {
	            var user = // Users should always 'exist' to the frontend, so create one if they don't, and return it
		            await _db.EconomyUsers.FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken) ??
		            await _mediator.Send(new AddEconomyUser.Request(request.UserId), cancellationToken);

	            var transactions = await _mediator.Send(new GetTransactions.Request(user.UserId), cancellationToken);
	            user.Transactions = transactions as List<EconomyTransaction>; // Probably shouldn't be casting, but it's a list anyway
	            
	            return user;
            }
        }
		
	}
}