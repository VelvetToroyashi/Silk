using System;
namespace SilkBot.Models
{
    [Flags]
    public enum UserPrivileges
    {
        Staff,
        Blacklisted,
        FreeShopOwner,
        PaidShopOwner,
        SilkPremiumUser,
    }
}