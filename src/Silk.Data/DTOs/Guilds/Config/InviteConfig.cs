using System.Collections.Generic;

namespace Silk.Data.DTOs.Guilds.Config;

public record InviteConfig
{
    public int                   Id                 { get; set; }
    public bool                  WhitelistEnabled   { get; set; }
    public bool                  UseAggressiveRegex { get; set; }
    public bool                  WarnOnMatch        { get; set; }
    public bool                  DeleteOnMatch      { get; set; }
    public bool                  ScanOrigin         { get; set; }
    public int                   GuildConfigId      { get; set; }
    public IReadOnlyList<Invite> Whitelist          { get; set; }
}