using System.Threading.Tasks;
using System.Timers;

namespace SilkBot.Economy.Shop.Utilities
{
    public sealed class ShopRefreshTimer
    {
        private readonly Timer timer = new Timer(3600000);

        public ShopRefreshTimer()
        {
            timer.Start();
            timer.Elapsed += (s, e) => Task.FromResult(-1);
        }
    }
}
