namespace Silk.Core.Database.Models
{
    public class Invite
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public string GuildName { get; set; }
        public string VanityURL { get; set; }
    }
}