using System;

namespace Silk.Core.Database.Models
{
    public class GuildInfractionModel
    {
        public int Id { get; set; }
        public GuildConfigModel Config { get; set; }
        public InfractionType Type { get; set; }
        public DateTime? Expiration { get; set; }
    }
}