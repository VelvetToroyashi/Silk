using System.Collections.Generic;
using Remora.Rest.Core;

namespace Silk.Data.DTOs.Guilds.Config;

public record GuildConfigDTO
(
    int                                            ID,
    int                                            MaxUserMentions,
    int                                            MaxRoleMentions,
    int                                            RaidDetectionThreshold,
    bool                                           UseNativeMute,
    bool                                           ProgressiveStriking,
    bool                                           DetectPhishingLinks,
    bool                                           BanSuspiciousUsernames,
    bool                                           DeletePhishingLinks,
    bool                                           EnableRaidDetection,
    Snowflake                                      GuildID,
    Snowflake                                      MuteRoleID,
    InviteConfigDTO                                Invites,
    GuildLoggingConfigDTO                          Logging,
    IReadOnlyList<GuildGreetingDTO>                Greetings,
    IReadOnlyList<ExemptionDTO>                    Exemptions,
    IReadOnlyList<InfractionStepDTO>               InfractionSteps,
    IReadOnlyDictionary<string, InfractionStepDTO> NamedInfractionSteps,
    int                                            RaidCooldownSeconds = 120
);