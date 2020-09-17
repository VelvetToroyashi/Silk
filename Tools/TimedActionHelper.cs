using System;
using System.Collections.Concurrent;
using System.Timers;

namespace SilkBot.Tools
{
    public class TimedActionHelper : IDisposable
    {
        private bool _disposed;
        
        public ConcurrentBag<TimedRestrictionAction> TimedRestrictedActions { get; } = new ConcurrentBag<TimedRestrictionAction>();

        public Timer Timer { get; } = new Timer(60000);
        
        public event EventHandler UnBan;
        public event EventHandler UnLock;
        public event EventHandler UnMute;
        //Region: placeholder (no functionality)
        public event EventHandler UnNoMeme;
        public event EventHandler UnVMute;
        //End Region: placeholder
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
                        UnMute?.Invoke(action, new EventArgs());
                        break;
                    case RestrictionActionReason.TemporaryLockout:
                        UnLock?.Invoke(action, new EventArgs());
                        break;
                }
            }
        }

        #region Dispose Pattern
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Timer?.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
