using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Silk.Data.Entities;

/// <summary>
/// Represents an infraction action to take depending on how many infractions a user has.
/// </summary>
[Table("infraction_steps")]
public class InfractionStepEntity
{
    /// <summary>
    /// The ID of this infraction step.
    /// </summary>
    public int                  Id     { get; set; }
    
    /// <summary>
    /// The ID of the config this step belongs to.
    /// </summary>
    [Column("config_id")]
    public int            ConfigId { get; set; }
    
    /// <summary>
    /// How many infractions this step requires.
    /// </summary>
    [Column("infraction_count")]
    public int Infractions { get; set; }
    
    /// <summary>
    /// What to do when this step is reached.
    /// </summary>
    [Column("infraction_type")]
    public InfractionType Type     { get; set; }
    
    /// <summary>
    /// How long this infraction step lasts.
    /// </summary>
    [Column("infraction_duration")]
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;
}