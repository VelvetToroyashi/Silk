using System.ComponentModel.DataAnnotations;

namespace RoleMenuPlugin.Database
{
	public sealed class RoleMenuOption
	{
		public ulong RoleId { get; set; }
		
		[Key]
		public ulong MessageId { get; set; }
		public string ComponentId { get; set; }
		public string EmojiName { get; set; }
		public string Description { get; set; }
	}

	public sealed record RoleMenuOptionDto
	{
		public ulong RoleId { get; init; }
		public ulong MessageId { get; init; }
		public string ComponentId { get; init; }
		public string EmojiName { get; init; }
		public string Description { get; init; }
	}
}