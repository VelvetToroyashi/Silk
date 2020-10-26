using System;
namespace SilkBot.Models
{
    [Flags]
    public enum UserFlag
    {
        Staff           = 1  ,
        WarnedPrior     = 2  ,
        KickedPrior     = 4  ,
        BannedPrior     = 8  ,
        Blacklisted     = 16 ,
        FreeShopOwner   = 32 ,
        PaidShopOwner   = 64 ,
        SilkPremiumUser = 128,
    }
}