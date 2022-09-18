using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Silk.Data.Entities;

public class GuildEntity
{
    /// <summary>
    /// The ID of the guild.
    /// </summary>
    public Snowflake ID { get; set; }

    /// <summary>
    /// The prefix on the guild.
    /// </summary>
    public string Prefix { get; set; } = "s!"; // No reference to Silk.Shared, so this is fine.
    
    /// <summary>
    /// Guild configuration.
    /// </summary>
    public GuildConfigEntity Configuration { get; set; } = new();
    
    /// <summary>
    /// Users that are a part of this guild.
    /// </summary>
    public List<GuildUserEntity> Users { get; set; } = new();
    
    /// <summary>
    /// Infractions that are a part of this guild.
    /// </summary>
    public List<InfractionEntity> Infractions { get; set; } = new();
    
}