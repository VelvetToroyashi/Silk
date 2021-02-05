using System;

namespace Silk.Core.Database.Models
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
        FreeShopOwner = 128,
        PaidShopOwner = 256,
        InfractionExemption = 512,
        Staff = 1024 | InfractionExemption,
        EscalatedStaff = 2048 | Staff
    }
}