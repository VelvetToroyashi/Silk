namespace Silk.Core.Data.Entities
{
    public class DisabledCommandEntity
    {
        public int         Id          { get; set; }
        public string      CommandName { get; set; }
        public ulong       GuildId     { get; set; }
        public GuildEntity Guild       { get; set; }
    }
}