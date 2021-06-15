using System.Collections.Generic;

namespace Silk.Core.Data.Models
{
	public sealed class RoleMenuMenu
	{
		public int Id { get; set; }
		public int GuildConfigId { get; set; }
		public string CategoryName { get; set; }
		public List<RoleMenuOption> Roles { get; set; } = new();
	}
}