using System;
using NpgsqlTypes;

namespace Silk.Core.Data.Entities;

/// <summary>
/// Represents an infraction action to take depending on how many infractions a user has.
/// </summary>
public class InfractionStepEntity
{
    /// <summary>
    /// The ID of this infraction step.
    /// </summary>
    public int                  Id     { get; set; }
    
    /// <summary>
    /// The ID of the config this step belongs to.
    /// </summary>
    public int            ConfigId { get; set; }
    
    /// <summary>
    /// How many infractions this step requires.
    /// </summary>
    public int Infractions { get; set; }
    
    /// <summary>
    /// What to do when this step is reached.
    /// </summary>
    public InfractionType Type     { get; set; }
    
    /// <summary>
    /// How long this infraction step lasts.
    /// </summary>
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;
}