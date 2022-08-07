using System.Collections.Generic;
using Remora.Rest.Core;

namespace Silk.Data.DTOs.Guilds.Config;

public record GuildConfig
{
    public int                                         Id                     { get; set; }
    public Snowflake                                   GuildID                { get; set; }
    public Snowflake                                   MuteRoleID             { get; set; }
    public int                                         MaxUserMentions        { get; set; }
    public int                                         MaxRoleMentions        { get; set; }
    public bool                                        UseNativeMute          { get; set; }
    public bool                                        ProgressiveStriking    { get; set; }
    public bool                                        DetectPhishingLinks    { get; set; }
    public bool                                        BanSuspiciousUsernames { get; set; }
    public bool                                        DeletePhishingLinks    { get; set; }
    public bool                                        EnableRaidDetection    { get; set; }
    public int                                         RaidDetectionThreshold { get; set; }
    public int                                         RaidCooldownSeconds    { get; set; } = 120;
    public InviteConfig                                Invites                { get; set; } = new();
    public GuildLoggingConfig                          Logging                { get; set; } = new();
    public IReadOnlyList<GuildGreeting>                Greetings              { get; set; }
    public IReadOnlyList<Exemption>                    Exemptions             { get; set; }
    public IReadOnlyList<InfractionStep>               InfractionSteps        { get; set; }
    public IReadOnlyDictionary<string, InfractionStep> NamedInfractionSteps   { get; set; }
}