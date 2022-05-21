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
    
    public ICollection<GuildEntity> Guilds { get; set; }
    
    /// <summary>
    /// Non-infraction related history of the user.
    /// </summary>
    public List<UserHistoryEntity> History { get; set; } = new();

    public List<InfractionEntity> Infractions { get; set; } = new();
    //public List<Reminder> Reminders { get; set; } = new();
}