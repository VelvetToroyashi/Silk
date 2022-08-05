using System.Collections.Generic;

namespace Silk.Data.DTOs.Guilds.Config;

public record InviteConfigDTO
(
    int                      ID,
    int                      ConfigID,
    bool                     WhitelistEnabled,
    bool                     UseAggressiveRegex,
    bool                     WarnOnMatch,
    bool                     DeleteOnMatch,
    bool                     ScanOrigin,
    IReadOnlyList<InviteDTO> Whitelist
);