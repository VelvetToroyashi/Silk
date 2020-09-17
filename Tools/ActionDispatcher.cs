using System;

namespace SilkBot.Tools
{
    public class ActionDispatcher
    {
        public event EventHandler UnBan;
        public event EventHandler UnLock;
        public event EventHandler UnMute;
        public event EventHandler UnNoMeme;
        public void Dispatch(AppEvent @event)
        {
            switch (@event.EventType)
            {
                case InfractionType.TemporaryBan:
                    UnBan?.Invoke(@event, EventArgs.Empty);
                    break;
                case InfractionType.TemporaryMute:
                    UnMute?.Invoke(@event, EventArgs.Empty);
                    break;
                case InfractionType.TemporaryLockout:
                    UnLock?.Invoke(@event, EventArgs.Empty);
                    break;
                case InfractionType.NoMeme:
                    UnNoMeme?.Invoke(@event, EventArgs.Empty);
                    break;
            }
        }
    }
}