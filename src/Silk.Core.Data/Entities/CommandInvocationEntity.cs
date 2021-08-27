using System.ComponentModel.DataAnnotations;

namespace Silk.Core.Data.Entities
{
    public class CommandInvocationEntity
    {
        public long Id { get; set; }

        public ulong UserId { get; set; }
        public ulong? GuildId { get; set; }

        [Required]
        public string CommandName { get; set; } = "";
    }
}