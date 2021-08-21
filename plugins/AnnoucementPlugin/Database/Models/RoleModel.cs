using System.ComponentModel.DataAnnotations;

namespace AnnoucementPlugin.Database
{
	public class Role
	{
		[Key]
		public int PK_Key { get; set; }
		public ulong Id { get; set; }
		
		public ulong GuildId { get; set; }
	}
}