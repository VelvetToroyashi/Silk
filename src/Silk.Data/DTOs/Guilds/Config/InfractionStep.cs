using System;
using Silk.Data.Entities;

namespace Silk.Data.DTOs.Guilds.Config;

public record InfractionStep
{
    public int            Id          { get; set; }
    public int            ConfigId    { get; set; }
    public int            Infractions { get; set; }
    public InfractionType Type        { get; set; }
    public TimeSpan       Duration    { get; set; } = TimeSpan.Zero;
}