namespace Silk.Core.Data.Entities
{
	public class InviteEntity
	{
		public int Id { get; set; }
		public ulong GuildId { get; set; }

		public ulong InviteGuildId { get; set; }
		public string VanityURL { get; set; }
	}
}