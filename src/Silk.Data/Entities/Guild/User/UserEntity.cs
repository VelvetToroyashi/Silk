using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Remora.Rest.Core;
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
    public List<GuildUserEntity> Guilds { get; set; } = new();
    
    /// <summary>
    /// Non-infraction related history of the user.
    /// </summary>
    public List<UserHistoryEntity> History { get; set; } = new();

    public List<InfractionEntity> Infractions { get; set; } = new();

    /// <summary>
    /// The timezone ID of the user (e.g. America/New_York). Null indicates the user has not set a timezone.
    /// </summary>
    public string? TimezoneID { get; set; } = null;
    
    /// <summary>
    /// Whether the user chooses to share their timezone,
    /// </summary>
    public bool ShareTimezone { get; set; }

    public static implicit operator User?(UserEntity? user) => ToDTO(user);
    
    public static User? ToDTO(UserEntity? user)
        => user is null ? null : new(user.ID,user.TimezoneID, user.ShareTimezone, user.Guilds.Select(g => g.GuildID).ToArray(), user.History.Select(UserHistoryEntity.ToDTO).ToArray(), user.Infractions.Select(InfractionEntity.ToDTO).ToArray()!);
}