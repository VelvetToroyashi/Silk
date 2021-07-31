using System;
using System.Collections.Generic;

namespace Silk.Core.Data.Models
{
	/// <summary>
	/// General history of a user. 
	/// </summary>
	public class UserHistory
	{
		public int Id { get; set; }
		/// <summary>
		/// The Id of the user this history is reflective of.
		/// </summary>
		public ulong UserId { get; set; }
		
		public User User { get; set; }
		
		/// <summary>
		/// The guild this history is related to.
		/// </summary>
		public ulong GuildId { get; set; }
		
		/// <summary>
		/// When this user initially joined.
		/// </summary>
		public DateTime JoinDate { get; set; }
		/// <summary>
		/// Times this user joined.
		/// </summary>
		public List<DateTime> JoinDates { get; set; }
		
		/// <summary>
		/// Times this user left.
		/// </summary>
		public List<DateTime> LeaveDates { get; set; }
		
	}
}