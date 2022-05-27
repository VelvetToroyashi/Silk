using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Silk.Data.Entities;

[Table("guild_user_joiner")]
public class GuildUserEntity
{
    [Column("user_id")]
    public Snowflake UserID  { get; set; }
    
    [Column("guild_id")]
    public Snowflake GuildID { get; set; }

    public UserEntity  User  { get; set; }
    public GuildEntity Guild { get; set; }
}