#region

using System;

#endregion

namespace Silk.Core.Database.Models
{
    [Flags]
    public enum UserFlag
    {
        None            = 0,
        WarnedPrior     = 2,
        KickedPrior     = 4,
        BannedPrior     = 8,
        Blacklisted     = 16,
        FreeShopOwner   = 32,
        PaidShopOwner   = 64,
        SilkPremiumUser = 128,
        Staff           = 4096,
        EscalatedStaff  = 8192 | Staff
    }
}