#region

using System.ComponentModel.DataAnnotations;

#endregion

namespace Silk.Core.Database.Models
{
    public class SelfAssignableRole
    {
        [Key] public ulong RoleId { get; set; }
    }
}