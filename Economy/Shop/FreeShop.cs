using System.Collections.Generic;

namespace SilkBot.Commands.Economy.Shop
{
    public sealed class FreeShop : BaseShop
    {
        public FreeShop() : base(10) {}

        //Free shops do not refresh hourly.//
        public override void CheckShopStatus() {}
    }
}
