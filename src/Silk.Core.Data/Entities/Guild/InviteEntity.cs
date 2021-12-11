using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Silk.Core.Data.Entities;

[Table("invites")]
public class InviteEntity
{
    public int       Id      { get; set; }
    
    [Column("guild_id")]
    public Snowflake GuildId { get; set; }

    [Column("invite_guild_id")]
    public Snowflake InviteGuildId { get; set; }
    
    [Column("invite_code")]
    public string VanityURL     { get; set; }
}