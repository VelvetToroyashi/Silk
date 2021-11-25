namespace RoleMenuPlugin.Database
{
	public sealed class RoleMenuOptionModel
	{
		public int Id { get; set; }

		public ulong RoleMenuId { get; set; }

		public ulong GuildId { get; set; }

		public ulong RoleId { get; set; }

		public ulong MessageId { get; set; }

		public string ComponentId { get; set; }

		public string? EmojiName { get; set; }

		public string? Description { get; set; }
	}

	public sealed record RoleMenuOptionDto
	{
		public ulong RoleId { get; init; }

		public string RoleName { get; init; }

		public ulong GuildId { get; init; }
		public ulong MessageId { get; init; }
		public string ComponentId { get; init; }
		public string? EmojiName { get; init; }
		public string? Description { get; init; }
	}
}