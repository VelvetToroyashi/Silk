using System.ComponentModel.DataAnnotations.Schema;

namespace RoleMenuPlugin.Database
{
	public sealed class RoleMenuOptionModel
	{
		[Column("RMO_Id")]
		public int Id { get; set; }
		
		[Column("RMO_FK")]
		public ulong RoleMenuId { get; set; }
		
		[Column("RMO_MessageId")]
		public ulong GuildId { get; set; }
		
		[Column("RMO_RoleId")]
		public ulong RoleId { get; set; }
		
		[Column("RMO_MessageId")]
		public ulong MessageId { get; set; }
		
		[Column("RMO_ComponentId")]
		public string ComponentId { get; set; }
		
		[Column("RMO_Emoji")]
		public string EmojiName { get; set; }
		
		[Column("RMO_Description")]
		public string Description { get; set; }
	}

	public sealed record RoleMenuOptionDto
	{
		public string RoleName { get; init; }
		public ulong RoleId { get; init; }
		public ulong GuildId { get; init; }
		public ulong MessageId { get; init; }
		public string ComponentId { get; init; }
		public string EmojiName { get; init; }
		public string Description { get; init; }
	}
}