using System.Timers;

namespace SilkBot.Tools
{
    public class TimingHelper
    {
        public Timer Timer { get; } = new Timer(60000);

        public TimingHelper()
        {
            Timer.Start();
        }
    }
}