using System;

namespace Silk.Api.Data.Models
{
	public class InfractionEntity
	{
		public int Id { get; set; }
		
		public Guid Key { get; set; }
		
		public InfractionType Type { get; set; }
		
		public ulong TargetUserId { get; set; }
		public ulong EnforcerUserId { get; set; }
		public ulong GuildCreationId { get; set; }
		
		public DateTime Created { get; set; }
		public DateTime Updated { get; set; }
		public DateTime? Expires { get; set; }
		
		public string Reason { get; set; }
		public bool IsPardoned { get; set; }
		
		
	}
}