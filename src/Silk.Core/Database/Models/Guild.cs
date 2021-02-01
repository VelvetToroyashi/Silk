using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Silk.Core.Database.Models
{
    public class Guild
    {
        public ulong Id { get; set; }

        [Required]
        [StringLength(5)]
        public string Prefix { get; set; }
        public GuildConfig Configuration { get; set; }
        public List<User> Users { get; set; } = new();
    }
}