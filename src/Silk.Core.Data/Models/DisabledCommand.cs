namespace Silk.Data.Models
{
    public class DisabledCommand
    {
        public int Id { get; set; }
        public string CommandName { get; set; }
        public ulong GuildId { get; set; }
        public Guild Guild { get; set; }
    }
}