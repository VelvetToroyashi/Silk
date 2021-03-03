using System.Collections.Generic;

namespace Silk.Data.Models
{
    public class User
    {
        public ulong Id { get; set; }
        public long DatabaseId { get; set; }
        public ulong GuildId { get; set; }
        public Guild Guild { get; set; } = null!;
        public UserFlag Flags { get; set; }
    }
}