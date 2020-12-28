using System.ComponentModel.DataAnnotations;

namespace Silk.Core.Database.Models
{
    public class GuildInviteModel
    {
        [Key]
        public int Id { get; set; }
        public string GuildName { get; set; }
        public string VanityURL { get; set; }
    }
}