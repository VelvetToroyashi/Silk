using NpgsqlTypes;

namespace Silk.Core.Data.Models
{
    public class InfractionStep
    {
        public int Id { get; set; }
        public GuildModConfig Config { get; set; } = null!;
        public InfractionType Type { get; set; }
        
        public NpgsqlTimeSpan Duration { get; set; } = NpgsqlTimeSpan.Zero;
    }
}