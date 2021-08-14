using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RoleMenuPlugin.Database
{
	/// <summary>
	/// A role menu model containing the message it belongs to and its options.
	/// </summary>
	public sealed class RoleMenuModel
	{
		[Key]
		public ulong MessageId { get; set; }

		public List<RoleMenuOption> Options { get; set; }
	}

	/// <summary>
	/// A role menu dto.
	/// </summary>
	public sealed record RoleMenuDto
	{
		public ulong MessageId { get; init; }
		
		public IReadOnlyList<RoleMenuOptionDto> Options { get; init; }
	}
}