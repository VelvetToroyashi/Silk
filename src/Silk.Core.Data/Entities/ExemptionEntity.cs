using System.ComponentModel.DataAnnotations.Schema;

namespace Silk.Core.Data.Entities
{
	public enum ExemptionType
	{
		Role,
		User,
		Channel
	}
	
	public sealed class ExemptionEntity
	{
		/// <summary>
		/// The Id of this exemption.
		/// </summary>
		public int Id { get; set; }
		
		/// <summary>
		/// What this exemption covers.
		/// </summary>
		[Column("exempt_from")]
		public string Exemption { get; set; }
		
		/// <summary>
		/// What type of exemption this is.
		/// </summary>
		[Column("type")]
		public ExemptionType Type { get; set; }
		
		/// <summary>
		/// The target of the exemption.
		/// </summary>
		[Column("target_id")]
		public ulong Target { get; set; }
		
		/// <summary>
		/// The guild this exemption applies to.
		/// </summary>
		[Column("guild_id")]
		public ulong Guild { get; set; }
	}
}