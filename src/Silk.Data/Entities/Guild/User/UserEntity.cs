using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Silk.Data.Entities;

[Table("users")]
public class UserEntity
{
    /// <summary>
    /// The ID of the user.
    /// </summary>
    [Column("id")]
    public Snowflake ID { get; set; }
    
    /// <summary>
    /// The ID of the user's guild.
    /// </summary>
    [Column("guild_id")]
    public Snowflake GuildID { get; set; }

    /// <summary>
    /// The guild this user belongs to.
    /// </summary>
    public GuildEntity Guild { get; set; } = null!;

    /// <summary>
    /// Any flags associated with this user.
    /// </summary>
    [Column("flags")]
    public UserFlag Flags { get; set; }
    
    /// <summary>
    /// Non-infraction related history of the user.
    /// </summary>
    public UserHistoryEntity History { get; set; }

    public List<InfractionEntity> Infractions { get; set; }
    //public List<Reminder> Reminders { get; set; } = new();
}