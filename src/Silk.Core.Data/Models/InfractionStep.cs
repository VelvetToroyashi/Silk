using NpgsqlTypes;

namespace Silk.Core.Data.Models
{
    public class InfractionStep
    {
        public int Id { get; set; }
        public GuildConfig Config { get; set; } = null!;
        public InfractionType Type { get; set; }
        //[Column(TypeName = "bigint")]
        public NpgsqlTimeSpan Expiration { get; set; } = NpgsqlTimeSpan.Zero;
    }
}