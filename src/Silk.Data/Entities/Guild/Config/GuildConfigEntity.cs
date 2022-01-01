using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Silk.Data.Entities;

[Table("guild_configs")]
public class GuildConfigEntity
{
    // Database requisites. //
    /// <summary>
    ///     The Primary Key (PK) of the model.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Requisite property to form a Foreign Key (FK)
    /// </summary>
    [Column("guild_id")]
    public Snowflake GuildID { get; set; }

    /// <summary>
    ///     Requisite property to form a Foreign Key (FK)
    /// </summary>
    public GuildEntity Guild { get; set; }
    
    /// <summary>
    /// Greetings configured for this guild.
    /// </summary>
    [Column("greetings")]
    public List<GuildGreetingEntity> Greetings { get; set; }
}