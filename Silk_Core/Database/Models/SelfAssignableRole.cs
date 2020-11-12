using System.ComponentModel.DataAnnotations;

namespace SilkBot.Models
{
    public class SelfAssignableRole
    {
        [Key]
        public ulong RoleId { get; set; }
    }
}
