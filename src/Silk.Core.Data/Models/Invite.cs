namespace Silk.Data.Models
{
    public class Invite
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public string VanityURL { get; set; }
    }
}