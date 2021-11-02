using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Silk.Economy.Data
{
	public static class RemoveEconomyUser
	{
		public sealed record Request(ulong UserId) : IRequest;
		
		public sealed class Handler : IRequestHandler<Request>
		{
			private readonly EconomyContext _db;
			
			public Handler(EconomyContext db) => _db = db;

			public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
			{
				var user = await _db.EconomyUsers.FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);
				var transactions = await _db.EconomyTransactions.Where(t => t.FromId == request.UserId).ToListAsync(cancellationToken);
				
				if (user == null)
                    return Unit.Value;
				
				_db.Remove(user);

				foreach (var transaction in transactions)
					transaction.FromId = ulong.MaxValue; // MaxValue is used to indicate that sender's account was deleted
				
				await _db.SaveChangesAsync(cancellationToken);
				
				return Unit.Value;
			}
		}
	}
}