using System;
using Silk.Data.Entities;

namespace Silk.Data.DTOs.Guilds.Config;

public record InfractionStepDTO
(
    int            ID,
    int            ConfigID,
    int            InfractionsRequired,
    InfractionType Type,
    TimeSpan       Duration
);