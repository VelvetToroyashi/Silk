using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Silk.Economy.Data;
using Silk.Economy.Data.Models;

namespace Silk.Economy.Core.Services
{
	public sealed class TransactionHelper
	{
		private readonly ILogger<TransactionHelper> _logger;
		private readonly IMediator _mediator;
		
		public TransactionHelper(ILogger<TransactionHelper> logger, IMediator mediator)
		{
			_logger = logger;
			_mediator = mediator;
		}

		public bool CanCommitTransaction(EconomyUser from, uint amount)
		{
			if (from.Flags.HasFlag(EconomyUserFlag.System))
				if (from.Balance is -1)
					return true;
			
			return from.Balance >= amount;
		}

		public async Task<EconomyTransaction> CreateTransactionAsync(EconomyUser from, EconomyUser to, uint amount, string? reason = "Monetary transfer.")
		{
			if (!from.Flags.HasFlag(EconomyUserFlag.System) && from.Balance != -1)
				from.Balance -= (int)amount;
			
			to.Balance += (int)amount;

			var trans = await _mediator.Send(new AddTransaction.Request(to.UserId, from.UserId, amount, reason));
			return trans;
		}
		
	}
}