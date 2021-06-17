namespace Silk.Core.Data.Models
{
	public class RoleMenuOption
	{
		public int Id { get; set; }
		public string RoleName { get; set; }
		public ulong RoleId { get; set; }
		public RoleMenuEmoji? Emoji { get; set; }

		public override string ToString() => $"Assign yourself the {RoleName} role.";
	}

	public sealed record RoleMenuEmoji(ulong? Id, string? Unicode);
}