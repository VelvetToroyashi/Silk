using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Economy.Data.Models;

namespace Silk.Economy.Data
{
	public class AddTransaction
	{
		public sealed record Request(ulong To, ulong From, uint Amount, string Reason) : IRequest<EconomyTransaction>;
		
		public sealed class Handler : IRequestHandler<Request, EconomyTransaction>
        {
            private readonly EconomyContext _db;

            public Handler(EconomyContext db) => _db = db;

            public async Task<EconomyTransaction> Handle(Request request, CancellationToken cancellationToken)
            {
                var transaction = new EconomyTransaction
                {
                    ToId = request.To,
                    FromId = request.From,
                    Amount = request.Amount,
                    Reason = request.Reason
                };

                _db.EconomyTransactions.Add(transaction);
                await _db.SaveChangesAsync(cancellationToken);

                return transaction;
            }
        }
	}
}