using System.Collections.Generic;

namespace Silk.Economy.Data.Models
{
	public class EconomyUser
	{
		public ulong UserId { get; set; }
		public string Motto { get; set; }
		public int Reputation { get; set; }
		public int Balance { get; set; }
		
		public List<EconomyTransaction> Transactions { get; set; } = new();
	}
}