using System;
using System.Collections.Concurrent;
using System.Timers;

namespace SilkBot.Tools
{
    public class TimerBatcher
    {
        public ConcurrentBag<AppEvent> Events { get; } = new ConcurrentBag<AppEvent>();
        public ActionDispatcher Dispatcher { get; private set; }
        
        public Timer Timer { get; } = new Timer(60000);
        
        public TimerBatcher(ActionDispatcher dispatcher)
        {
            // Set before start of Timer (could throw if timer Ticks before delegate is added to EventHandler)
            this.Dispatcher = dispatcher;
            
            Timer.Start();
            Timer.Elapsed += OnTimerTick;
        }

        private void OnTimerTick(object s, ElapsedEventArgs e)
        {
            foreach (var @event in Events)
            {
                if (DateTime.Now > @event.Expiration)
                {
                    Dispatcher.Dispatch(@event);
                }
            }
        }
    }
}
