using System;
using System.Collections.Concurrent;
using System.Timers;

namespace SilkBot.Tools
{
    public class TimedEventService
    {
        public ConcurrentBag<ITimedEvent> Events { get; } = new ConcurrentBag<ITimedEvent>();

        
        private Timer _timer = new Timer(60000);
        
        public TimedEventService()
        {
            
            _timer.Start();
            _timer.Elapsed += OnTimerTick;
        }

        private void OnTimerTick(object s, ElapsedEventArgs e)
        {
            foreach (var tEvent in Events)
                if (DateTime.Now > tEvent.Expiration)
                    tEvent.Callback.Invoke(tEvent);
        }
    }
}
