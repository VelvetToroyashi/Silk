using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Silk.Data.Entities;

public class GuildUserEntity
{
    public Snowflake UserID  { get; set; }
    
    public Snowflake GuildID { get; set; }

    public UserEntity  User  { get; set; }
    public GuildEntity Guild { get; set; }
}