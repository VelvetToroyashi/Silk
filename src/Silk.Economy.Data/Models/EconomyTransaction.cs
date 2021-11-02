using System;

namespace Silk.Economy.Data.Models
{
	/// <summary>
	/// Represetns a transaction between two users.
	/// </summary>
	public record EconomyTransaction
	{
		/// <summary>
		/// An identifier for the transaction for database purposes.
		/// </summary>
		public int Id { get; init; }
		
		/// <summary>
		/// A unique identifier for the transaction.
		/// </summary>
		public Guid TransactionId { get; init; } = Guid.NewGuid();
		
		/// <summary>
		/// The user who made the transaction. Transactions with SYSTEM will always have an Id of 0.
		/// </summary>
		public ulong FromId { get; set; }
		
		/// <summary>
		/// The ID of the user who is receiving the money.
		/// </summary>
		public ulong ToId { get; init; }
		
		/// <summary>
		/// The amount of money that was transferred.
		/// </summary>
		public uint Amount { get; init; }
		
		/// <summary>
		/// Why the transaction was made. This is set automatically, except in the case of refunds and transfers.
		/// </summary>
		public string Reason { get; init; }
		
		/// <summary>
		/// Whether the transaction is voided. A voided transaction is one that has been reversed.
		/// </summary>
		public bool IsVoided { get; set; }
		
		/// <summary>
		/// Whether the transaction is valid. Invalid transactions do not display in the transaction history. 
		/// </summary>
		public bool IsValid { get; set; }
		
		/// <summary>
		/// The date and time the transaction was made.
		/// </summary>
		public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
	}
}