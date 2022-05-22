using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Remora.Rest.Core;
using Silk.Data.DTOs.Guilds;
using Silk.Data.DTOs.Guilds.Users;

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
    /// The guilds this user is a part of.
    /// </summary>
    public ICollection<GuildEntity> Guilds { get; set; }
    
    /// <summary>
    /// Non-infraction related history of the user.
    /// </summary>
    public List<UserHistoryEntity> History { get; set; } = new();

    public List<InfractionEntity> Infractions { get; set; } = new();

    public static implicit operator UserDTO?(UserEntity? user) => ToDTO(user);
    
    public static UserDTO? ToDTO(UserEntity? user)
        => user is null ? null : new(user.ID, user.Guilds.Select(g => g.ID).ToArray(), user.History.Select(UserHistoryEntity.ToDTO).ToArray(), user.Infractions.Select(InfractionEntity.ToDTO).ToArray());
}