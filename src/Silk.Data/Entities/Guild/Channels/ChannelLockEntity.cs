using System;
using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Silk.Data.Entities.Channels;

[Table("channel_locks")]
public class ChannelLockEntity
{
    [Column("id")]
    public int Id { get; set; }

    [Column("channel_id")]
    public Snowflake ChannelID { get; set; }
    
    [Column("guild_id")]
    public Snowflake GuildID { get; set; }
    
    [Column("locked_by")]
    public Snowflake UserID { get; set; }
    
    [Column("locked_roles")]
    public Snowflake[] LockedRoles { get; set; }
    
    [Column("unlocks_at")]
    public DateTimeOffset? UnlocksAt { get; set; }
    
    [Column("reason")]
    public string Reason { get; set; }
}