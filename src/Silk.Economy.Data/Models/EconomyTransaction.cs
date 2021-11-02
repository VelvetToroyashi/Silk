using System;

namespace Silk.Economy.Data.Models
{
	public record EconomyTransaction
	{
		public int Id { get; init; }
		public Guid TransactionId { get; init; } = Guid.NewGuid();
		public ulong FromId { get; init; }
		public ulong ToId { get; init; }
		
		public EconomyUser From { get; init; }
		public EconomyUser To { get; init; }
		
		public uint Amount { get; init; }
		public string Reason { get; init; }
		public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
	}
}