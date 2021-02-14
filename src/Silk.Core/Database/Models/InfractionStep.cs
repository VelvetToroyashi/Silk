using System;

namespace Silk.Core.Database.Models
{
    public class InfractionStep
    {
        public int Id { get; set; }
        public GuildConfig Config { get; set; } = null!;
        public InfractionType Type { get; set; }
        public DateTime? Expiration { get; set; }
    }
}