using System;
using System.ComponentModel.DataAnnotations.Schema;
using Silk.Api.Data.Entities;

namespace Silk.Api.Data.Models
{
	public class InfractionEntity
	{
		[Column("Api_Infraction_PK")]
		public int Id { get; set; }
		
		[Column("Api_Infraction_Key")]
		public Guid Key { get; set; }
		
		[Column("Api_Infraction_Type")]
		public InfractionType Type { get; set; }
		
		[Column("Api_Infraction_Target")]
		public ulong TargetUserId { get; set; }
		
		[Column("Api_Infraction_Enforcer")]
		public ulong EnforcerUserId { get; set; }
		
		[Column("Api_Infraction_Guild")]
		public ulong GuildCreationId { get; set; }
		
		[Column("Api_Infraction_Creation")]
		public DateTime Created { get; set; }
		
		[Column("Api_Infraction_Updated")]
		public DateTime Updated { get; set; }
		
		[Column("Api_Infraction_Expiration")]
		public DateTime? Expires { get; set; }
		
		[Column("Api_Infraction_Reason")]
		public string Reason { get; set; }
		
		[Column("Api_Infraction_Pardon")]
		public bool IsPardoned { get; set; }
		
		[Column("Api_Infraction_AddedBy")]
		public ApiUser AddedBy { get; set; }
		
		[Column("Api_Infraction_AddedBy_FK")]
		public string AddedByFK { get; set; }
	}
}