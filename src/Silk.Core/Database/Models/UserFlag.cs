using System;

namespace Silk.Core.Database.Models
{
    [Flags]
    public enum UserFlag
    {
        None = 0,
        WarnedPrior = 2,
        KickedPrior = 4 | WarnedPrior,
        BannedPrior = 8 | WarnedPrior,
        Blacklisted = 16,
        FreeShopOwner = 32,
        PaidShopOwner = 64 | FreeShopOwner,
        SilkPremiumUser = 128,
        InfractionExemption = 256,
        Staff = 4096 | InfractionExemption,
        EscalatedStaff = 8192 | Staff
    }
}