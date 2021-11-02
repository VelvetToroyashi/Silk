using System;

namespace Silk.Economy.Data.Models
{
	[Flags]
	public enum EconomyUserFlag
	{
		/// <summary>
		/// User has no special flags.
		/// </summary>
		None = 0, 
		
		/// <summary>
		/// Developer account. Not currently anything special.
		/// </summary>
		Developer = 1,
		
		/// <summary>
		/// System account. Allows for <see cref="EconomyUser.Balance"/> to be set to -1, allowing for infinite money.
		/// </summary>
		System = 2,
	}
}