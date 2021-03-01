using System;

namespace Silk.Data.Models
{
    [Flags]
    public enum UserFlag
    {
        None = 0,
        ActivelyMuted = 2,
        ActivelyBanned = 4,
        WarnedPrior = 8,
        KickedPrior = 16,
        BannedPrior = 32,
        Blacklisted = 64,
        InfractionExemption = 128,
        Staff = 256 | InfractionExemption, //1536
        EscalatedStaff = 512 | Staff  // 3584
    }
}