using System;
namespace SilkBot.Models
{
    [Flags]
    public enum UserFlag
    {
        WarnedPrior     = 2,
        KickedPrior     = 4,
        BannedPrior     = WarnedPrior & KickedPrior,
        Blacklisted     = 16,
        FreeShopOwner   = 32,
        PaidShopOwner   = 64,
        SilkPremiumUser = 128,
        Staff           = 4096,
        EscalatedStaff  = 8192
    }
}