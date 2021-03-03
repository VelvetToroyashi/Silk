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
        InfractionExemption = 64,
        Staff = 128 | InfractionExemption, //1536
        EscalatedStaff = 256 | Staff  // 3584
    }
}