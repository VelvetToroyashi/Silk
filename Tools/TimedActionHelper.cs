using System;
using System.Collections.Concurrent;
using System.Timers;

namespace SilkBot.Tools
{
    public class TimedActionHelper
    {
        public ConcurrentBag<TimedRestrictionAction> TimedRestrictedActions { get; } = new ConcurrentBag<TimedRestrictionAction>();

        public Timer Timer { get; } = new Timer(60000);
        
        public event EventHandler UnBan;
        public event EventHandler Unlock;
        public event EventHandler Unmute;
        
        public TimedActionHelper()
        {
            Timer.Start();
            Timer.Elapsed += OnTimerTick;
        }

        private void OnTimerTick(object s, ElapsedEventArgs e)
        {
            foreach (var action in TimedRestrictedActions)
            {
                if (DateTime.Now < action.Expiration)
                {
                    continue;
                }

                switch (action.ActionReason)
                {
                    case RestrictionActionReason.TemporaryBan:
                        UnBan?.Invoke(action, new EventArgs());
                        break;
                    case RestrictionActionReason.TemporaryMute:
                        Unmute?.Invoke(action, new EventArgs());
                        break;
                    case RestrictionActionReason.TemporaryLockout:
                        Unlock?.Invoke(action, new EventArgs());
                        break;
                }
            }
        }
    }
}
