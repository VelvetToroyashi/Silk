using System.ComponentModel.DataAnnotations;

namespace Silk.Data.Models
{
    public class CommandInvocation
    {
        public long Id { get; set; }
        
        public ulong UserId { get; set; }
        public ulong? GuildId { get; set; }
        
        [Required]
        public string CommandName { get; set; } = "";

    }
}