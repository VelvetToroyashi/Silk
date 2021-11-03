using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Economy.Data.Models;

namespace Silk.Economy.Data
{
	public static class UpdateTransaction
	{
		public sealed record Request(string Id, bool IsVoided) : IRequest<EconomyTransaction>;
		
		public sealed class Handler : IRequestHandler<Request, EconomyTransaction>
		{
			private readonly EconomyContext _db;
			public Handler(EconomyContext db) => _db = db;

			public async Task<EconomyTransaction> Handle(Request request, CancellationToken cancellationToken)
			{
				var transaction = await _db.EconomyTransactions.FirstOrDefaultAsync(tr => tr.TransactionId == request.Id, cancellationToken);
				
				if (transaction == null)
                    throw new ArgumentException($"Transaction with id {request.Id} not found.");

				transaction.IsVoided = request.IsVoided;
				
				await _db.SaveChangesAsync(cancellationToken);
				
				return transaction;
			}
		}
	}
}