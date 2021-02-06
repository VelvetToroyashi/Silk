using System.Collections.Generic;

namespace Silk.Core.Database.Models
{
    public class User
    {
        public ulong Id { get; set; }
        public long DatabaseId { get; set; }
        public Guild Guild { get; set; }
        public UserFlag Flags { get; set; }
        public List<Infraction> Infractions { get; set; } = new();
    }
}  