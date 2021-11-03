using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Silk.Economy.Data.Models;

namespace Silk.Economy.Data
{
	public static class GetTransactions
	{
		public record Request(ulong UserId) : IRequest<IEnumerable<EconomyTransaction>>;
		
		public class Handler : IRequestHandler<Request, IEnumerable<EconomyTransaction>>
		{
			private readonly EconomyContext _db;
			
			public Handler(EconomyContext db) => _db = db;

			public async Task<IEnumerable<EconomyTransaction>> Handle(Request request, CancellationToken cancellationToken)
			{
				return await _db.EconomyTransactions.Where(x => x.ToId == request.UserId || x.FromId == request.UserId).ToListAsync(cancellationToken);
			}
		}
	}
}