using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoleMenuPlugin.Database
{
	/// <summary>
	/// A role menu model containing the message it belongs to and its options.
	/// </summary>
	public sealed class RoleMenuModel
	{
		[Key]
		[Column("RM_MessageId")]
		public ulong MessageId { get; set; }
		
		[Column("RM_GuildId")]
		public ulong GuildId { get; set; }
		
		public List<RoleMenuOptionModel> Options { get; set; }
	}

	/// <summary>
	/// A role menu dto.
	/// </summary>
	public sealed record RoleMenuDto
	{
		public ulong MessageId { get; init; }
		public ulong GuildId { get; init; }

		public IReadOnlyList<RoleMenuOptionDto> Options { get; init; }
	}
}