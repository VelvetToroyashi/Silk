using System;
using System.Collections.Concurrent;
using System.Timers;

namespace SilkBot.Tools
{
    public class TimerBatcher
    {
        public ConcurrentBag<AppEvent> Events { get; } = new ConcurrentBag<AppEvent>();
        private ActionDispatcher dispatcher;
        public Timer Timer { get; } = new Timer(60000);
        


        //End Region: placeholder
        public TimerBatcher(ActionDispatcher dispatcher)
        {
            Timer.Start();
            Timer.Elapsed += OnTimerTick;
            this.dispatcher = dispatcher;
        }

        private void OnTimerTick(object s, ElapsedEventArgs e)
        {
            foreach (var @event in Events)
            {
                if (DateTime.Now > @event.Expiration)
                {
                    dispatcher.Dispatch(@event);
                }
            }
        }
    }
}
