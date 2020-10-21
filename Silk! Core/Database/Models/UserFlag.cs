using System;
namespace SilkBot.Models
{
    [Flags]
    public enum UserFlag
    {
        Staff,
        WarnedPrior,
        KickedPrior,
        BannedPrior,
        Blacklisted,
        FreeShopOwner,
        PaidShopOwner,
        SilkPremiumUser,
    }
}