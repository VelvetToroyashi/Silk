using NpgsqlTypes;

namespace Silk.Core.Data.Entities
{
    public class InfractionStepEntity
    {
        public int Id { get; set; }
        public GuildModConfigEntity Config { get; set; } = null!;
        public InfractionType Type { get; set; }
        
        public NpgsqlTimeSpan Duration { get; set; } = NpgsqlTimeSpan.Zero;
    }
}