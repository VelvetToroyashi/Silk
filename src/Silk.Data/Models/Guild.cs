using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Silk.Data.Models
{
    public class Guild
    {
        public ulong Id { get; set; }

        [Required]
        [StringLength(5)]
        public string Prefix { get; set; } = "";
        public GuildConfig Configuration { get; set; } = new();
        public List<User> Users { get; set; } = new();
    }
}