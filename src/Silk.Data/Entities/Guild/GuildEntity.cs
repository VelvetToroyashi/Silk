using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Remora.Rest.Core;

namespace Silk.Data.Entities;

[Table("guilds")]
public class GuildEntity
{
    /// <summary>
    /// The ID of the guild.
    /// </summary>
    [Column("Id")] // Backwards compatibility?
    public Snowflake ID { get; set; }

    /// <summary>
    /// The prefix on the guild.
    /// </summary>
    [Required]
    [StringLength(5)]
    [Column("prefix")]
    public string Prefix                        { get; set; } = "";
    
    /// <summary>
    /// Non-moderation related configuration.
    /// </summary>
    public GuildConfigEntity      Configuration { get; set; } = new();
    
    /// <summary>
    /// Moderation-related configuration.
    /// </summary>
    public GuildModConfigEntity   ModConfig     { get; set; } = new();
    
    /// <summary>
    /// Users that are a part of this guild.
    /// </summary>
    public List<UserEntity>       Users         { get; set; } = new();
    
    /// <summary>
    /// Infractions that are a part of this guild.
    /// </summary>
    public List<InfractionEntity> Infractions   { get; set; } = new();
    
    /// <summary>
    /// Tags on this guild.
    /// </summary>
    public List<TagEntity>        Tags          { get; set; } = new();
}