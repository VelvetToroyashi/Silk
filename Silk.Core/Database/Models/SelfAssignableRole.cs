using System.ComponentModel.DataAnnotations;

namespace Silk.Core.Database.Models
{
    public class SelfAssignableRole
    {
        [Key] public ulong RoleId { get; set; }
    }
}